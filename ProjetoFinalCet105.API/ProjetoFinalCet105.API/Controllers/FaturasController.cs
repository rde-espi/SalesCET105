using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinalCet105.API.Services.Faturacao;
using ProjetoFinalCet105.API.UseCases.Common;
using ProjetoFinalCet105.API.UseCases.Faturas;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FaturasController : ControllerBase
    {
        private readonly CreateFaturaUseCase _createFaturaUseCase;
        private readonly GetFaturaByIdUseCase _getFaturaByIdUseCase;
        private readonly GetFaturasUseCase _getFaturasUseCase;
        private readonly AnularFaturaUseCase _anularFaturaUseCase;
        private readonly IFaturaPdfService _faturaPdfService;

        public FaturasController(CreateFaturaUseCase createFaturaUseCase, GetFaturaByIdUseCase getFaturaByIdUseCase, GetFaturasUseCase getFaturasUseCase, AnularFaturaUseCase anularFaturaUseCase,
            IFaturaPdfService faturaPdfService)
        {
            _createFaturaUseCase = createFaturaUseCase;
            _getFaturaByIdUseCase = getFaturaByIdUseCase;
            _getFaturasUseCase = getFaturasUseCase;
            _anularFaturaUseCase = anularFaturaUseCase;
            _faturaPdfService = faturaPdfService;
        }

        [HttpPost("marcacao/{marcacaoId:int}")]
        [Authorize(Roles = "Admin,Funcionario")]
        public async Task<IActionResult> Create(int marcacaoId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isFuncionario = User.IsInRole("Funcionario");

            var resultado = await _createFaturaUseCase.ExecuteAsync(marcacaoId, userId, isFuncionario, isAdmin);
            
            if (!resultado.Sucesso)
            {
                return resultado.TipoErro switch
                {
                    TipoErro.NaoEncontrado => NotFound(resultado.Erro),

                    TipoErro.Proibido => Forbid(),

                    _ => BadRequest(resultado.Erro)
                };
            }

            return CreatedAtAction( nameof(Create), new { marcacaoId }, resultado.Dados);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isFuncionario = User.IsInRole("Funcionario");
            bool isCliente = User.IsInRole("Cliente");

            var resultado = await _getFaturaByIdUseCase.ExecuteAsync( id,userId,isCliente,isFuncionario,isAdmin);

            if (!resultado.Sucesso)
            {
                return resultado.TipoErro switch
                {
                    TipoErro.NaoEncontrado => NotFound(resultado.Erro),

                    TipoErro.Proibido => Forbid(),

                    _ => BadRequest(resultado.Erro)
                };
            }

            return Ok(resultado.Dados);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] DateTime? dataInicio = null,
            [FromQuery] DateTime? dataFim = null,
            [FromQuery] string? numero = null,
            [FromQuery] string? estado = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isFuncionario = User.IsInRole("Funcionario");
            bool isCliente = User.IsInRole("Cliente");

            var resultado = await _getFaturasUseCase.ExecuteAsync(
                userId,
                isCliente,
                isFuncionario,
                isAdmin,
                dataInicio,
                dataFim,
                numero,
                estado);

            if (!resultado.Sucesso)
            {
                return resultado.TipoErro switch
                {
                    TipoErro.NaoEncontrado => NotFound(resultado.Erro),

                    TipoErro.Proibido => Forbid(),

                    _ => BadRequest(resultado.Erro)
                };
            }

            return Ok(resultado.Dados);
        }

        [HttpGet("{id:int}/pdf")]
        public async Task<IActionResult> GetPdf(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isFuncionario = User.IsInRole("Funcionario");
            bool isCliente = User.IsInRole("Cliente");

            var resultado = await _getFaturaByIdUseCase.ExecuteAsync(
                id,
                userId,
                isCliente,
                isFuncionario,
                isAdmin);

            if (!resultado.Sucesso)
            {
                return resultado.TipoErro switch
                {
                    TipoErro.NaoEncontrado => NotFound(resultado.Erro),

                    TipoErro.Proibido => Forbid(),

                    _ => BadRequest(resultado.Erro)
                };
            }

            var fatura = resultado.Dados!;

            var pdfBytes = _faturaPdfService.GerarPdf(fatura);

            var nomeFicheiro = $"Fatura-{fatura.Numero.Replace("/", "-")}.pdf";

            return File( pdfBytes, "application/pdf", nomeFicheiro);
        }

        [HttpPatch("{id:int}/anular")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Anular(int id)
        {
            bool isAdmin = User.IsInRole("Admin");

            var resultado = await _anularFaturaUseCase.ExecuteAsync( id, isAdmin);

            if (!resultado.Sucesso)
            {
                return resultado.TipoErro switch
                {
                    TipoErro.NaoEncontrado => NotFound(resultado.Erro),
                    TipoErro.Proibido => Forbid(),
                    _ => BadRequest(resultado.Erro)
                };
            }

            return Ok(new
            {
                mensagem = "Fatura anulada com sucesso."
            });
        }
    }
}
