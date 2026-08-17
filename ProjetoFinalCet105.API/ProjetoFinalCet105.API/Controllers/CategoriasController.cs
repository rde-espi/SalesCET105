using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepository _categoriaRepository;
        public CategoriasController(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetAllCategorias()
        {
            var categorias = await _categoriaRepository.GetAll().ToListAsync();
            return Ok(categorias);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);
            if(categoria == null)
            {
                return NotFound();
            }
            return Ok(categoria);
        }

        [HttpPost]
        public async Task<ActionResult<Categoria>> CreateCategoria(Categoria categoria)
        {
            try
            {
                await _categoriaRepository.CreateAsync(categoria);

                return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria);
            }
            catch (Exception) 
            {
                return BadRequest();
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategoria(int id, Categoria categoria)
        {
            if(id != categoria.Id)
            {
                return BadRequest();
            }
            if(!await _categoriaRepository.ExistAsync(id))
            {
                return NotFound();
            }

            try
            {
                await _categoriaRepository.UpdateAsync(categoria);
            }
            catch (Exception) 
            {
                return BadRequest();
            }
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            try
            {
                await _categoriaRepository.DeleteAsync(categoria);
            }
            catch (Exception)
            {
                return BadRequest();
            }

            return NoContent();
        }
    }
}
