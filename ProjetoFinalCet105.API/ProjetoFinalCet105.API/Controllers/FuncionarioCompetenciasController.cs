using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FuncionarioCompetenciaDTO>>>
            GetAllFuncionarioCompetencias()
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

        [HttpGet("{id:int}")]
        public async Task<ActionResult<FuncionarioCompetenciaDTO>>
            GetFuncionarioCompetenciaById(int id)
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

        [HttpPost]
        public async Task<ActionResult<FuncionarioCompetenciaDTO>>
            CreateFuncionarioCompetencia(FuncionarioCompetenciaDTO dto)
        {
            if (!await _funcionarioRepository.ExistAsync(dto.FuncionarioId))
            {
                return BadRequest("O funcionário indicado não existe.");
            }

            if (!await _competenciaRepository.ExistAsync(dto.CompetenciaId))
            {
                return BadRequest("A competência indicada não existe.");
            }

            try
            {
                var fc = new FuncionarioCompetencia
                {
                    FuncionarioId = dto.FuncionarioId,
                    CompetenciaId = dto.CompetenciaId,
                    Nivel = dto.Nivel,
                    Certificacao = dto.Certificacao
                };

                await _funcionarioCompetenciaRepository.CreateAsync(fc);

                dto.Id = fc.Id;

                var funcionario = await _funcionarioRepository
                    .GetFuncionarioByIdAsync(dto.FuncionarioId);

                var competencia = await _competenciaRepository
                    .GetByIdAsync(dto.CompetenciaId);

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

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFuncionarioCompetencia(
            int id,
            FuncionarioCompetenciaDTO dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            if (!await _funcionarioCompetenciaRepository.ExistAsync(id))
            {
                return NotFound();
            }

            if (!await _funcionarioRepository.ExistAsync(dto.FuncionarioId))
            {
                return BadRequest("O funcionário indicado não existe.");
            }

            if (!await _competenciaRepository.ExistAsync(dto.CompetenciaId))
            {
                return BadRequest("A competência indicada não existe.");
            }

            try
            {
                var fc = new FuncionarioCompetencia
                {
                    Id = id,
                    FuncionarioId = dto.FuncionarioId,
                    CompetenciaId = dto.CompetenciaId,
                    Nivel = dto.Nivel,
                    Certificacao = dto.Certificacao
                };

                await _funcionarioCompetenciaRepository.UpdateAsync(fc);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFuncionarioCompetencia(int id)
        {
            var fc = await _funcionarioCompetenciaRepository.GetByIdAsync(id);

            if (fc == null)
            {
                return NotFound();
            }

            try
            {
                await _funcionarioCompetenciaRepository.DeleteAsync(fc);
            }
            catch (Exception)
            {
                return BadRequest();
            }

            return NoContent();
        }
    }
}
