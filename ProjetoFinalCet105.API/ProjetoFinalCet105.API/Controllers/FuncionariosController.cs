using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Funcionarios;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuncionariosController : BaseApiController
    {
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IServicoRepository _servicoRepository;
        private readonly IFuncionarioServicoRepository _funcionarioServicoRepository;
        private readonly CreateFuncionarioUseCase _createFuncionarioUseCase;
        private readonly UpdateFuncionarioUseCase _updateFuncionarioUseCase;
        private readonly DeleteFuncionarioUseCase _deleteFuncionarioUseCase;

        public FuncionariosController(IFuncionarioRepository funcionarioRepository,
            IServicoRepository servicoRepository,
            IFuncionarioServicoRepository funcionarioServicoRepository,
            CreateFuncionarioUseCase createFuncionarioUseCase,
            UpdateFuncionarioUseCase updateFuncionarioUseCase,
            DeleteFuncionarioUseCase deleteFuncionarioUseCase)
        {
            _funcionarioRepository = funcionarioRepository;
            _servicoRepository = servicoRepository;
            _funcionarioServicoRepository = funcionarioServicoRepository;
            _createFuncionarioUseCase = createFuncionarioUseCase;
            _updateFuncionarioUseCase = updateFuncionarioUseCase;
            _deleteFuncionarioUseCase = deleteFuncionarioUseCase;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FuncionarioDTO>>> GetAllFuncionarios()
        {
            var funcionarios = await _funcionarioRepository.GetAllFuncionariosWithUser()
                .Select(f => new FuncionarioDTO
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    NomeCompleto = f.User.NomeCompleto,
                    Email = f.User.Email,
                    Telefone = f.User.PhoneNumber,
                    FotografiaUrl = f.User.FotografiaUrl,
                    Biografia = f.Biografia,
                    DataAdmissao = f.DataAdmissao,
                    Disponivel = f.Disponivel,
                    Ativo = f.Ativo
                })
                .ToListAsync();
            return Ok(funcionarios);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<FuncionarioDTO>> GetFuncionarioById(int id)
        {
            var funcionario = await _funcionarioRepository.GetFuncionarioByIdAsync(id);
            if (funcionario == null)
            {
                return NotFound();
            }
            return Ok(new FuncionarioDTO
            {
                Id = funcionario.Id,
                UserId = funcionario.UserId,
                NomeCompleto = funcionario.User.NomeCompleto,
                Email = funcionario.User.Email,
                Telefone = funcionario.User.PhoneNumber,
                FotografiaUrl = funcionario.User.FotografiaUrl,
                Biografia = funcionario.Biografia,
                DataAdmissao = funcionario.DataAdmissao,
                Disponivel = funcionario.Disponivel,
                Ativo = funcionario.Ativo
            });
        }

        [Authorize(Policy = "AlterarFuncionario")]
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<FuncionarioDTO>> GetFuncionarioByUserId(string userId)
        {
            var authenticatedUserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (authenticatedUserId == null)
            {
                return Unauthorized();
            }

            if (User.IsInRole("Funcionario") &&
                !User.IsInRole("Admin") &&
                authenticatedUserId != userId)
            {
                return Forbid();
            }

            var funcionario =
                await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

            if (funcionario == null)
            {
                return NotFound();
            }

            return Ok(new FuncionarioDTO
            {
                Id = funcionario.Id,
                UserId = funcionario.UserId,
                NomeCompleto = funcionario.User.NomeCompleto,
                Email = funcionario.User.Email,
                Telefone = funcionario.User.PhoneNumber,
                FotografiaUrl = funcionario.User.FotografiaUrl,
                Biografia = funcionario.Biografia,
                DataAdmissao = funcionario.DataAdmissao,
                Disponivel = funcionario.Disponivel,
                Ativo = funcionario.Ativo
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<FuncionarioDTO>> CreateFuncionario(NovoFuncionarioDTO dto)
        {
            var resultado =
                await _createFuncionarioUseCase.ExecuteAsync(dto);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return CreatedAtAction(
                nameof(GetFuncionarioById),
                new { id = resultado.Dados!.Id },
                resultado.Dados);
        }

        [Authorize(Policy = "AlterarFuncionario")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFuncionario(int id, UpdateFuncionarioDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado =
                await _updateFuncionarioUseCase.ExecuteAsync(
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

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFuncionario(int id)
        {
            var resultado =
                await _deleteFuncionarioUseCase.ExecuteAsync(id);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return NoContent();
        }

        [HttpGet("servico/{servicoId:int}")]
        public async Task<ActionResult<IEnumerable<FuncionarioDTO>>> GetFuncionariosByServico(int servicoId)
        {
            var servico = await _servicoRepository.GetByIdAsync(servicoId);

            if (servico == null)
            {
                return NotFound("Serviço não encontrado.");
            }

            var funcionarios = await _funcionarioServicoRepository
                .GetAllWithDetails()
                .Where(fs =>
                fs.ServicoId == servicoId &&
                fs.Ativo &&
                fs.Funcionario.Ativo &&
                fs.Funcionario.Disponivel)
                .Select(fs => new FuncionarioDTO
                {
                    Id = fs.Funcionario.Id,
                    UserId = fs.Funcionario.UserId,
                    NomeCompleto = fs.Funcionario.User.NomeCompleto,
                    Email = fs.Funcionario.User.Email,
                    Telefone = fs.Funcionario.User.PhoneNumber,
                    FotografiaUrl = fs.Funcionario.User.FotografiaUrl,
                    Biografia = fs.Funcionario.Biografia,
                    DataAdmissao = fs.Funcionario.DataAdmissao,
                    Disponivel = fs.Funcionario.Disponivel,
                    Ativo = fs.Funcionario.Ativo
                })
                .ToListAsync();
            return Ok(funcionarios);
        }
    }
}
