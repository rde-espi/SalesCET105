using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Models;
using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicosController : ControllerBase
    {
        private readonly IServicoRepository _servicoRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public ServicosController(IServicoRepository servicoRepository, ICategoriaRepository categoriaRepository)
        {
            _servicoRepository = servicoRepository;
            _categoriaRepository = categoriaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServicoDTO>>> GetAllServicosWithCategoria()
        {
            var servicos = await _servicoRepository
                .GetAllWithCategoria()
                .Select(s => new ServicoDTO
                {
                    Id = s.Id,
                    CategoriaId = s.CategoriaId,
                    CategoriaNome = s.Categoria!.Nome,
                    Nome = s.Nome,
                    Descricao = s.Descricao,
                    Preco = s.Preco,
                    DuracaoMinutos = s.DuracaoMinutos,
                    Disponivel = s.Disponivel,
                    DataCriacao = s.DataCriacao,
                    DataAtualizacao = s.DataAtualizacao
                })
                .ToListAsync();
            return Ok(servicos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServicoDTO>> GetServicoByIdWithCategoria(int id)
        {
            var servico = await _servicoRepository.GetByIdWithCategoriaAsync(id);
            if (servico == null)
            {
                return NotFound();
            }

            return Ok(new ServicoDTO
            {
                Id = servico.Id,
                CategoriaId = servico.CategoriaId,
                CategoriaNome = servico.Categoria!.Nome,
                Nome = servico.Nome,
                Descricao = servico.Descricao,
                Preco = servico.Preco,
                DuracaoMinutos = servico.DuracaoMinutos,
                Disponivel = servico.Disponivel,
                DataCriacao = servico.DataCriacao,
                DataAtualizacao = servico.DataAtualizacao
            });
        }

        [HttpGet("{id:int}/imagem")]
        public async Task<IActionResult> GetImagemServico(int id)
        {
            var servico = await _servicoRepository.GetByIdAsync(id);

            if (servico == null)
            {
                return NotFound();
            }

            if (servico.Imagem == null ||
                servico.Imagem.Length == 0 ||
                string.IsNullOrWhiteSpace(servico.ImagemContentType))
            {
                return NotFound();
            }

            return File(servico.Imagem, servico.ImagemContentType);
        }

        [Authorize(Policy = "AdminOuAdminTemporario")]
        [HttpPost]
        public async Task<ActionResult<ServicoDTO>> CreateServico([FromFormAttribute] ServicoFormModel model)
        {
            if (!await _categoriaRepository.ExistAsync(model.CategoriaId))
            {
                return BadRequest("Categoria indicada não existe");
            }

            if (model.Preco < 0)
            {
                return BadRequest("O preço do serviço não pode ser negativo.");
            }

            if (model.DuracaoMinutos <= 0)
            {
                return BadRequest("A duração do serviço deve ser superior a zero.");
            }

            if (string.IsNullOrWhiteSpace(model.Nome))
            {
                return BadRequest("O nome do serviço é obrigatório.");
            }

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

                var servico = new Servico
                {
                    CategoriaId = model.CategoriaId,
                    Nome = model.Nome,
                    Descricao = model.Descricao,
                    Preco = model.Preco,
                    DuracaoMinutos = model.DuracaoMinutos,
                    Imagem = imagemBytes,
                    ImagemContentType = imagemContentType,
                    Disponivel = true,
                    DataCriacao = DateTime.Now
                };

                await _servicoRepository.CreateAsync(servico);

                var categoria = await _categoriaRepository.GetByIdAsync(servico.CategoriaId);

                var dto = new ServicoDTO
                {
                    Id = servico.Id,
                    CategoriaId = servico.CategoriaId,
                    CategoriaNome = categoria!.Nome,
                    Nome = servico.Nome,
                    Descricao = servico.Descricao,
                    Preco = servico.Preco,
                    DuracaoMinutos = servico.DuracaoMinutos,
                    Disponivel = servico.Disponivel,
                    DataCriacao = servico.DataCriacao
                };

                return CreatedAtAction(nameof(GetServicoByIdWithCategoria), new { id = servico.Id }, dto);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize(Policy = "AdminOuAdminTemporario")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateServico([FromForm] ServicoFormModel model, int id)
        {
            if (!await _servicoRepository.ExistAsync(id))
            {
                return NotFound();
            }

            if (!await _categoriaRepository.ExistAsync(model.CategoriaId))
            {
                return BadRequest("A categoria indicada não existe");
            }

            if (model.Preco < 0)
            {
                return BadRequest("O preço do serviço não pode ser negativo.");
            }

            if (model.DuracaoMinutos <= 0)
            {
                return BadRequest("A duração do serviço deve ser superior a zero.");
            }

            if (string.IsNullOrWhiteSpace(model.Nome))
            {
                return BadRequest("O nome do serviço é obrigatório.");
            }

            try
            {
                var servicoAtual = await _servicoRepository.GetByIdAsync(id);

                if (servicoAtual == null)
                {
                    return NotFound();
                }

                servicoAtual.CategoriaId = model.CategoriaId;
                servicoAtual.Nome = model.Nome;
                servicoAtual.Descricao = model.Descricao;
                servicoAtual.Preco = model.Preco;
                servicoAtual.DuracaoMinutos = model.DuracaoMinutos;
                servicoAtual.DataAtualizacao = DateTime.Now;

                if (model.Imagem != null && model.Imagem.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await model.Imagem.CopyToAsync(memoryStream);

                    servicoAtual.Imagem = memoryStream.ToArray();
                    servicoAtual.ImagemContentType = model.Imagem.ContentType;
                }

                await _servicoRepository.UpdateAsync(servicoAtual);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize(Policy = "AdminOuAdminTemporario")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteServico(int id)
        {
            var servico = await _servicoRepository.GetByIdAsync(id);

            if (servico == null)
            {
                return NotFound();
            }

            if (!servico.Disponivel)
            {
                return BadRequest("O serviço já se encontra indisponível.");
            }

            try
            {
                servico.Disponivel = false;
                servico.DataAtualizacao = DateTime.Now;

                await _servicoRepository.UpdateAsync(servico);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize(Policy = "AdminOuAdminTemporario")]
        [HttpPatch("{id:int}/ativar")]
        public async Task<IActionResult> AtivarServico(int id)
        {
            var servico = await _servicoRepository.GetByIdAsync(id);

            if (servico == null)
            {
                return NotFound();
            }

            if (servico.Disponivel)
            {
                return BadRequest("O serviço já se encontra disponível.");
            }

            try
            {
                servico.Disponivel = true;
                servico.DataAtualizacao = DateTime.Now;

                await _servicoRepository.UpdateAsync(servico);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}