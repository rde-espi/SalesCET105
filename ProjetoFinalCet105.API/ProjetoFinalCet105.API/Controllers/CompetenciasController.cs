using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetenciasController : ControllerBase
    {
        private readonly ICompetenciaRepository _competenciaRepository;

        public CompetenciasController(ICompetenciaRepository competenciaRepository)
        {
            _competenciaRepository = competenciaRepository;
        }
        
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Competencia>>> GetAllCompetencias()
        {
            var competencias = await _competenciaRepository.GetAll().ToListAsync();

            return Ok(competencias);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Competencia>> GetCompetenciaById(int id)
        {
            var competencia = await _competenciaRepository.GetByIdAsync(id);
            if (competencia == null)
            {
                return NotFound();
            }
            return Ok(competencia);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Competencia>> CreateCompetencia(Competencia competencia)
        {
            if (string.IsNullOrWhiteSpace(competencia.Nome))
            {
                return BadRequest("O nome da competência é obrigatório.");
            }
            try
            {
                competencia.Ativa = true;
                await _competenciaRepository.CreateAsync(competencia);
                return CreatedAtAction(nameof(GetCompetenciaById), new { id = competencia.Id }, competencia);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCompetencia(int id, Competencia competencia)
        {
            if (id != competencia.Id)
            {
                return BadRequest();
            }
            if (!await _competenciaRepository.ExistAsync(id))
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(competencia.Nome))
            {
                return BadRequest("O nome da competência é obrigatório.");
            }
            try
            {
                await _competenciaRepository.UpdateAsync(competencia);
            }
            catch (Exception)
            {
                return BadRequest();
            }
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCompetencia(int id)
        {
            var competencia = await _competenciaRepository.GetByIdAsync(id);

            if (competencia == null)
            {
                return NotFound();
            }

            if (!competencia.Ativa)
            {
                return BadRequest("A competência já se encontra inativa.");
            }

            try
            {
                competencia.Ativa = false;

                await _competenciaRepository.UpdateAsync(competencia);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
