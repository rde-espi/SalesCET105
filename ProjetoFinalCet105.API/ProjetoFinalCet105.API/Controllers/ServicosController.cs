using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
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
                    ImagemUrl = s.ImagemUrl,
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
                ImagemUrl = servico.ImagemUrl,
                Disponivel = servico.Disponivel,
                DataCriacao = servico.DataCriacao,
                DataAtualizacao = servico.DataAtualizacao
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ServicoDTO>> CreateServico(ServicoDTO dto)
        {
            if (!await _categoriaRepository.ExistAsync(dto.CategoriaId))
            {
                return BadRequest("Categoria indicada não existe");
            }

            if (dto.Preco < 0)
            {
                return BadRequest("O preço do serviço não pode ser negativo.");
            }

            if (dto.DuracaoMinutos <= 0)
            {
                return BadRequest("A duração do serviço deve ser superior a zero.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nome))
            {
                return BadRequest("O nome do serviço é obrigatório.");
            }

            try
            {
                var servico = new Servico
                {
                    CategoriaId = dto.CategoriaId,
                    Nome = dto.Nome,
                    Descricao = dto.Descricao,
                    Preco = dto.Preco,
                    DuracaoMinutos = dto.DuracaoMinutos,
                    ImagemUrl = dto.ImagemUrl,
                    Disponivel = dto.Disponivel,
                    DataCriacao = DateTime.Now
                };

                await _servicoRepository.CreateAsync(servico);

                dto.Id = servico.Id;
                dto.DataCriacao = servico.DataCriacao;

                var categoria = await _categoriaRepository.GetByIdAsync(servico.CategoriaId);

                dto.CategoriaNome = categoria!.Nome;

                return CreatedAtAction(nameof(GetServicoByIdWithCategoria), new { id = servico.Id }, dto);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateServico(ServicoDTO dto, int id)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }
            if (!await _servicoRepository.ExistAsync(id))
            {
                return NotFound();
            }
            if (!await _categoriaRepository.ExistAsync(dto.CategoriaId))
            {
                return BadRequest("A categoria indicada não existe");
            }

            if (dto.Preco < 0)
            {
                return BadRequest("O preço do serviço não pode ser negativo.");
            }

            if (dto.DuracaoMinutos <= 0)
            {
                return BadRequest("A duração do serviço deve ser superior a zero.");
            }

            if (string.IsNullOrWhiteSpace(dto.Nome))
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
                var servico = new Servico
                {
                    Id = id,
                    CategoriaId = dto.CategoriaId,
                    Nome = dto.Nome,
                    Descricao = dto.Descricao,
                    Preco = dto.Preco,
                    DuracaoMinutos = dto.DuracaoMinutos,
                    ImagemUrl = dto.ImagemUrl,
                    Disponivel = dto.Disponivel,
                    DataCriacao = servicoAtual.DataCriacao,
                    DataAtualizacao = DateTime.Now
                };
                await _servicoRepository.UpdateAsync(servico);
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }

        }

        [Authorize(Roles = "Admin")]
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




    }
}
