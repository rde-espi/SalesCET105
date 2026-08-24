using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuncionarioCompetenciasController : ControllerBase
    {
        private readonly IFuncionarioCompetenciaRepository _funcionarioCompetenciaRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly ICompetenciaRepository _competenciaRepository;

        public FuncionarioCompetenciasController(
            IFuncionarioCompetenciaRepository funcionarioCompetenciaRepository,
            IFuncionarioRepository funcionarioRepository,
            ICompetenciaRepository competenciaRepository)
        {
            _funcionarioCompetenciaRepository = funcionarioCompetenciaRepository;
            _funcionarioRepository = funcionarioRepository;
            _competenciaRepository = competenciaRepository;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FuncionarioCompetenciaDTO>>>GetAllFuncionarioCompetencias()
        {
            var dados = await _funcionarioCompetenciaRepository
                .GetAllWithDetails()
                .Select(fc => new FuncionarioCompetenciaDTO
                {
                    Id = fc.Id,
                    FuncionarioId = fc.FuncionarioId,
                    FuncionarioNome = fc.Funcionario.User.NomeCompleto,
                    CompetenciaId = fc.CompetenciaId,
                    CompetenciaNome = fc.Competencia.Nome,
                    Nivel = fc.Nivel,
                    Certificacao = fc.Certificacao
                }).ToListAsync();                

            return Ok(dados);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FuncionarioCompetenciaDTO>>GetFuncionarioCompetenciaById(int id)
        {
            var fc = await _funcionarioCompetenciaRepository
                .GetByIdWithDetailsAsync(id);

            if (fc == null)
            {
                return NotFound();
            }

            return Ok(new FuncionarioCompetenciaDTO
            {
                Id = fc.Id,
                FuncionarioId = fc.FuncionarioId,
                FuncionarioNome = fc.Funcionario.User.NomeCompleto,
                CompetenciaId = fc.CompetenciaId,
                CompetenciaNome = fc.Competencia.Nome,
                Nivel = fc.Nivel,
                Certificacao = fc.Certificacao
            });
        }
        
        [Authorize(Policy = "GerirCompetenciasFuncionario")]
        [HttpPost]
        public async Task<ActionResult<FuncionarioCompetenciaDTO>>CreateFuncionarioCompetencia(FuncionarioCompetenciaDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            int funcionarioId;

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

                funcionarioId = funcionarioAutenticado.Id;
            }
            else
            {
                funcionarioId = dto.FuncionarioId;
            }

            if (!await _funcionarioRepository.ExistAsync(funcionarioId))
            {
                return BadRequest("O funcionário indicado não existe.");
            }

            if (!await _competenciaRepository.ExistAsync(dto.CompetenciaId))
            {
                return BadRequest("A competência indicada não existe.");
            }

            if (await _funcionarioCompetenciaRepository.ExisteFuncionarioCompetenciaAsync(funcionarioId,dto.CompetenciaId))
            {
                return BadRequest(
                    "O funcionário já possui esta competência.");
            }
            try
            {
                var fc = new FuncionarioCompetencia
                {
                    FuncionarioId = funcionarioId,
                    CompetenciaId = dto.CompetenciaId,
                    Nivel = dto.Nivel,
                    Certificacao = dto.Certificacao
                };

                await _funcionarioCompetenciaRepository.CreateAsync(fc);

                dto.Id = fc.Id;

                var funcionario = await _funcionarioRepository
                    .GetFuncionarioByIdAsync(funcionarioId);

                var competencia = await _competenciaRepository
                    .GetByIdAsync(dto.CompetenciaId);

                dto.FuncionarioId = funcionarioId;
                dto.FuncionarioNome = funcionario!.User.NomeCompleto;
                dto.CompetenciaNome = competencia!.Nome;

                return CreatedAtAction(
                    nameof(GetFuncionarioCompetenciaById),
                    new { id = fc.Id },
                    dto);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize(Policy = "GerirCompetenciasFuncionario")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFuncionarioCompetencia(int id,FuncionarioCompetenciaDTO dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            // Obter a associação que realmente existe
            var fc =
                await _funcionarioCompetenciaRepository.GetByIdAsync(id);

            if (fc == null)
            {
                return NotFound();
            }

            // A associação continua a pertencer ao mesmo funcionário
            var funcionarioId = fc.FuncionarioId;

            // Funcionário só pode alterar as próprias competências
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

                if (fc.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return Forbid();
                }
            }

            if (!await _competenciaRepository.ExistAsync(dto.CompetenciaId))
            {
                return BadRequest(
                    "A competência indicada não existe.");
            }

            if (await _funcionarioCompetenciaRepository
                .ExisteFuncionarioCompetenciaAsync(
                    funcionarioId,
                    dto.CompetenciaId,
                    id))
            {
                return BadRequest(
                    "O funcionário já possui esta competência.");
            }

            try
            {
                fc.CompetenciaId = dto.CompetenciaId;
                fc.Nivel = dto.Nivel;
                fc.Certificacao = dto.Certificacao;

                await _funcionarioCompetenciaRepository
                    .UpdateAsync(fc);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize(Policy = "GerirCompetenciasFuncionario")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFuncionarioCompetencia(int id)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var fc =
                await _funcionarioCompetenciaRepository.GetByIdAsync(id);

            if (fc == null)
            {
                return NotFound();
            }

            // Funcionário só pode eliminar as próprias competências
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

                if (fc.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return Forbid();
                }
            }

            try
            {
                await _funcionarioCompetenciaRepository.DeleteAsync(fc);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
