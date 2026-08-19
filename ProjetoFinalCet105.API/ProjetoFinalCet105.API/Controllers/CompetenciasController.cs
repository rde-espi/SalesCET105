using Microsoft.AspNetCore.Http;
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
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Competencia>>> GetAllCompetencias()
        {
            var competencias= await _competenciaRepository.GetAll().ToListAsync();
            
            return Ok(competencias);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Competencia>>GetCompetenciaById(int id)
        {
            var competencia = await _competenciaRepository.GetByIdAsync(id);
            if (competencia == null)
            {
                return NotFound();
            }
            return Ok(competencia);
        }

        [HttpPost]
        public async Task<ActionResult<Competencia>>CreateCompetencia(Competencia competencia)
        {
            try
            {
                await _competenciaRepository.CreateAsync(competencia);
                return CreatedAtAction(nameof(GetCompetenciaById), new { id = competencia.Id }, competencia);
            }
            catch(Exception)
            {
                return BadRequest();
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult>UpdateCompetencia(int id, Competencia competencia)
        {
            if(id != competencia.Id)
            {
                return BadRequest();
            }
            if(!await _competenciaRepository.ExistAsync(id))
            {
                return NotFound();
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

        [HttpDelete("{id:int}")]
        public async Task<IActionResult>DeleteCompetencia(int id)
        {
            var competencia = await _competenciaRepository.GetByIdAsync(id);
            if(competencia == null)
            {
                return NotFound();
            }
            try
            {
                await _competenciaRepository.DeleteAsync(competencia);
            }
            catch (Exception)
            {
                return BadRequest();
            }
            return NoContent();
        }
    }
}
