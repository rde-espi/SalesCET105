using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.HorariosFuncionarios;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HorarioFuncionariosController : BaseApiController
    {
        private readonly IHorarioFuncionarioRepository _horarioFuncionarioRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly CreateHorarioFuncionarioUseCase _createHorarioFuncionarioUseCase;
        private readonly UpdateHorarioFuncionarioUseCase _updateHorarioFuncionarioUseCase;
        private readonly DeleteHorarioFuncionarioUseCase _deleteHorarioFuncionarioUseCase;

        public HorarioFuncionariosController(IHorarioFuncionarioRepository horarioFuncionarioRepository, IFuncionarioRepository funcionarioRepository,
            CreateHorarioFuncionarioUseCase createHorarioFuncionarioUseCase, UpdateHorarioFuncionarioUseCase updateHorarioFuncionarioUseCase, 
            DeleteHorarioFuncionarioUseCase deleteHorarioFuncionarioUseCase)
        {
            _horarioFuncionarioRepository = horarioFuncionarioRepository;
            _funcionarioRepository = funcionarioRepository;
            _createHorarioFuncionarioUseCase = createHorarioFuncionarioUseCase;
            _updateHorarioFuncionarioUseCase = updateHorarioFuncionarioUseCase;
            _deleteHorarioFuncionarioUseCase = deleteHorarioFuncionarioUseCase;
        }

        [Authorize(Policy = "ConsultarHorario")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HorarioFuncionarioDTO>>> GetAllHorariosFuncionarios()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var query =
                _horarioFuncionarioRepository.GetAllWithFuncionario();

            if (User.IsInRole("Funcionario") &&
                !User.IsInRole("Admin"))
            {
                var funcionario =
                    await _funcionarioRepository
                        .GetFuncionarioByUserIdAsync(userId);

                if (funcionario == null)
                {
                    return Forbid();
                }

                query = query.Where(h =>
                    h.FuncionarioId == funcionario.Id);
            }

            var horarios = await query
                .OrderBy(h => h.DiaSemana)
                .ThenBy(h => h.HoraInicio)
                .Select(h => new HorarioFuncionarioDTO
                {
                    Id = h.Id,
                    FuncionarioId = h.FuncionarioId,
                    FuncionarioNome =
                        h.Funcionario.User.NomeCompleto,

                    DiaSemana = h.DiaSemana,

                    HoraInicio = h.HoraInicio,
                    HoraFim = h.HoraFim,

                    Ativo = h.Ativo
                })
                .ToListAsync();

            return Ok(horarios);
        }

        [Authorize(Policy = "ConsultarHorario")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<HorarioFuncionarioDTO>> GetHorarioFuncionarioById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var horario = await _horarioFuncionarioRepository.GetByIdWithFuncionarioAsync(id);
            if (horario == null)
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

                if (horario.FuncionarioId !=
                    funcionarioAutenticado.Id)
                {
                    return Forbid();
                }
            }

            return Ok(new HorarioFuncionarioDTO
            {
                Id = horario.Id,
                FuncionarioId = horario.FuncionarioId,
                FuncionarioNome = horario.Funcionario.User.NomeCompleto,
                DiaSemana = horario.DiaSemana,
                HoraInicio = horario.HoraInicio,
                HoraFim = horario.HoraFim,
                Ativo = horario.Ativo
            });
        }

        [Authorize(Policy = "GerirHorario")]
        [HttpPost]
        public async Task<ActionResult<HorarioFuncionarioDTO>> CreateHorarioFuncionario(NovoHorarioFuncionarioDTO dto)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado =
                await _createHorarioFuncionarioUseCase.ExecuteAsync(
                    userId,
                    User.IsInRole("Funcionario"),
                    User.IsInRole("Admin"),
                    dto);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return CreatedAtAction(
                nameof(GetHorarioFuncionarioById),
                new { id = resultado.Dados!.Id },
                resultado.Dados);
        }

        [Authorize(Policy = "GerirHorario")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateHorarioFuncionario(int id,UpdateHorarioFuncionarioDTO dto)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado =
                await _updateHorarioFuncionarioUseCase.ExecuteAsync(
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


        [Authorize(Policy = "GerirHorario")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHorarioFuncionario(int id)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado =
                await _deleteHorarioFuncionarioUseCase.ExecuteAsync(
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
