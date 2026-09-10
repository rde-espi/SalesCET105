using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Models;
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
        public async Task<ActionResult<Categoria>> GetCategoriaById(int id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }
            return Ok(categoria);
        }

        [HttpGet("{id:int}/imagem")]
        public async Task<IActionResult> GetImagemCategoria(int id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            if (categoria.Imagem == null ||
                categoria.Imagem.Length == 0 ||
                string.IsNullOrWhiteSpace(categoria.ImagemContentType))
            {
                return NotFound();
            }

            return File(
                categoria.Imagem,
                categoria.ImagemContentType);
        }

        [Authorize(Policy = "AdminOuAdminTemporario")]
        [HttpPost]
        public async Task<ActionResult<Categoria>> CreateCategoria([FromForm] CategoriaFormModel model)
        {
            try
            {
                byte[]? imagemBytes = null;
                string? imagemContentType = null;

                if (model.Imagem != null && model.Imagem.Length > 0)
                {
                    using var memoryStream = new MemoryStream();

                    await model.Imagem.CopyToAsync(memoryStream);

                    imagemBytes = memoryStream.ToArray();
                    imagemContentType = model.Imagem.ContentType;
                }

                var categoria = new Categoria
                {
                    Nome = model.Nome,
                    Descricao = model.Descricao,
                    Imagem = imagemBytes,
                    ImagemContentType = imagemContentType,
                    Ativa = true
                };

                await _categoriaRepository.CreateAsync(categoria);

                return CreatedAtAction(
                    nameof(GetCategoriaById),
                    new { id = categoria.Id },
                    categoria);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize(Policy = "AdminOuAdminTemporario")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategoria( int id, [FromForm] CategoriaFormModel model)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            try
            {
                categoria.Nome = model.Nome;
                categoria.Descricao = model.Descricao;

                // Só altera a imagem se for enviado um novo ficheiro
                if (model.Imagem != null && model.Imagem.Length > 0)
                {
                    using var memoryStream = new MemoryStream();

                    await model.Imagem.CopyToAsync(memoryStream);

                    categoria.Imagem = memoryStream.ToArray();
                    categoria.ImagemContentType = model.Imagem.ContentType;
                }

                await _categoriaRepository.UpdateAsync(categoria);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize(Policy = "AdminOuAdminTemporario")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            if (!categoria.Ativa)
            {
                return BadRequest("A categoria já se encontra inativa.");
            }

            try
            {
                categoria.Ativa = false;

                await _categoriaRepository.UpdateAsync(categoria);

                return NoContent();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
