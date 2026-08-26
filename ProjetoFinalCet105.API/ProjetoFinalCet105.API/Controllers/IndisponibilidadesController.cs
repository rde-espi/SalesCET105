using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Indisponibilidades;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndisponibilidadesController : BaseApiController
    {
        private readonly IIndisponibilidadeRepository _indisponibilidadeRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly CreateIndisponibilidadeUseCase _createIndisponibilidadeUseCase;
        private readonly UpdateIndisponibilidadeUseCase _updateIndisponibilidadeUseCase;
        private readonly DeleteIndisponibilidadeUseCase _deleteIndisponibilidadeUseCase;

        public IndisponibilidadesController(IIndisponibilidadeRepository indisponibilidadeRepository,IFuncionarioRepository funcionarioRepository,
            CreateIndisponibilidadeUseCase createIndisponibilidadeUseCase, UpdateIndisponibilidadeUseCase updateIndisponibilidadeUseCase, DeleteIndisponibilidadeUseCase deleteIndisponibilidadeUseCase)
        {
            _indisponibilidadeRepository = indisponibilidadeRepository;
            _funcionarioRepository = funcionarioRepository;
            _createIndisponibilidadeUseCase = createIndisponibilidadeUseCase;
            _updateIndisponibilidadeUseCase = updateIndisponibilidadeUseCase;
            _deleteIndisponibilidadeUseCase = deleteIndisponibilidadeUseCase;
        }

        [Authorize(Policy = "ConsultarIndisponibilidades")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IndisponibilidadeDTO>>>GetAllIndisponibilidadesWithFuncionario()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var query = _indisponibilidadeRepository.GetAllIndisponibilidadesWithFuncionario();

            if (User.IsInRole("Funcionario") &&
                !User.IsInRole("Admin"))
            {
                var funcionarioAutenticado =
                    await _funcionarioRepository
                        .GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return Forbid();
                }

                query = query.Where(i =>
                    i.FuncionarioId == funcionarioAutenticado.Id);
            }

            var indisponibilidades = await query
                .OrderBy(i => i.DataHoraInicio)
                .Select(i => new IndisponibilidadeDTO
                {
                    Id = i.Id,
                    FuncionarioId = i.FuncionarioId,
                    FuncionarioNome = i.Funcionario.User.NomeCompleto,
                    DataHoraInicio = i.DataHoraInicio,
                    DataHoraFim = i.DataHoraFim,
                    Motivo = i.Motivo,
                    DiaCompleto = i.DiaCompleto,
                    RestoDoDia = i.RestoDoDia
                })
                .ToListAsync();

            return Ok(indisponibilidades);
        }

        [Authorize(Policy = "ConsultarIndisponibilidades")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<IndisponibilidadeDTO>>GetIndisponibilidadeWithFuncionarioById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var indisponibilidade = await _indisponibilidadeRepository.GetIndisponibilidadeWithFuncionarioByIdAsync(id);

            if(indisponibilidade == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Funcionario") && !User.IsInRole("Admin"))
            {
                var funcionarioAutenticado = await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return Forbid();
                }

                if (indisponibilidade.FuncionarioId !=
                    funcionarioAutenticado.Id)
                {
                    return Forbid();
                }
            }

            return Ok(new IndisponibilidadeDTO
            {
                Id = indisponibilidade.Id,
                FuncionarioId = indisponibilidade.FuncionarioId,
                FuncionarioNome = indisponibilidade.Funcionario.User.NomeCompleto,
                DataHoraInicio = indisponibilidade.DataHoraInicio,
                DataHoraFim = indisponibilidade.DataHoraFim,
                Motivo = indisponibilidade.Motivo,
                DiaCompleto = indisponibilidade.DiaCompleto,
                RestoDoDia= indisponibilidade.RestoDoDia
            });
        }

        [Authorize(Policy = "GerirIndisponibilidades")]
        [HttpPost]
        public async Task<ActionResult<IndisponibilidadeDTO>> CreateIndisponibilidade(NovaIndisponibilidadeDTO dto)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado =
                await _createIndisponibilidadeUseCase.ExecuteAsync(
                    userId,
                    User.IsInRole("Funcionario"),
                    User.IsInRole("Admin"),
                    dto);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return CreatedAtAction(nameof(GetIndisponibilidadeWithFuncionarioById),
                new { id = resultado.Dados!.Id },
                resultado.Dados);
        }

        [Authorize(Policy = "GerirIndisponibilidades")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateIndisponibilidade( int id, UpdateIndisponibilidadeDTO dto)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado =
                await _updateIndisponibilidadeUseCase.ExecuteAsync(
                    id,
                    userId,
                    User.IsInRole("Funcionario"),
                    User.IsInRole("Admin"),
                    dto);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return NoContent();
        }

        [Authorize(Policy = "GerirIndisponibilidades")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteIndisponibilidade(int id)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado =
                await _deleteIndisponibilidadeUseCase.ExecuteAsync(
                    id,
                    userId,
                    User.IsInRole("Funcionario"),
                    User.IsInRole("Admin"));

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return NoContent();
        }
    }
}
