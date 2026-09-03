using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin")]
    public class DespesasController : ControllerBase
    {
        private readonly IDespesaRepository _despesaRepository;

        public DespesasController(IDespesaRepository despesaRepository)
        {
            _despesaRepository = despesaRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NovaDespesaDTO dto)
        {
            if (dto.DataDespesa == default)
                return BadRequest("A data da despesa é obrigatória.");

            var despesa = new Despesa
            {
                Descricao = dto.Descricao.Trim(),
                Valor = dto.Valor,
                DataDespesa = dto.DataDespesa,
                Categoria = string.IsNullOrWhiteSpace(dto.Categoria)
                    ? null
                    : dto.Categoria.Trim(),
                Observacoes = string.IsNullOrWhiteSpace(dto.Observacoes)
                    ? null
                    : dto.Observacoes.Trim(),
                DataCriacao = DateTime.UtcNow
            };

            await _despesaRepository.CreateAsync(despesa);

            var resultado = MapearDTO(despesa);

            return CreatedAtAction( nameof(GetById), new { id = despesa.Id }, resultado);
        }

        [HttpGet]
        public async Task<ActionResult<List<DespesaDTO>>> GetAll()
        {
            var despesas = await _despesaRepository
                .GetAllDespesas()
                .OrderByDescending(d => d.DataDespesa)
                .Select(d => new DespesaDTO
                {
                    Id = d.Id,
                    Descricao = d.Descricao,
                    Valor = d.Valor,
                    DataDespesa = d.DataDespesa,
                    Categoria = d.Categoria,
                    Observacoes = d.Observacoes,
                    DataCriacao = d.DataCriacao
                })
                .ToListAsync();

            return Ok(despesas);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var despesa = await _despesaRepository.GetByIdAsync(id);

            if (despesa == null)
                return NotFound("Despesa não encontrada.");

            return Ok(MapearDTO(despesa));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDespesaDTO dto)
        {
            if (dto.DataDespesa == default)
                return BadRequest("A data da despesa é obrigatória.");

            var despesa = await _despesaRepository.GetByIdAsync(id);

            if (despesa == null)
                return NotFound("Despesa não encontrada.");

            despesa.Descricao = dto.Descricao.Trim();
            despesa.Valor = dto.Valor;
            despesa.DataDespesa = dto.DataDespesa;
            despesa.Categoria = string.IsNullOrWhiteSpace(dto.Categoria)
                ? null
                : dto.Categoria.Trim();

            despesa.Observacoes = string.IsNullOrWhiteSpace(dto.Observacoes)
                ? null
                : dto.Observacoes.Trim();

            await _despesaRepository.UpdateAsync(despesa);

            return Ok(MapearDTO(despesa));
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var despesa = await _despesaRepository.GetByIdAsync(id);

            if (despesa == null)
                return NotFound("Despesa não encontrada.");

            await _despesaRepository.DeleteAsync(despesa);

            return NoContent();
        }

        private static DespesaDTO MapearDTO(Despesa despesa)
        {
            return new DespesaDTO
            {
                Id = despesa.Id,
                Descricao = despesa.Descricao,
                Valor = despesa.Valor,
                DataDespesa = despesa.DataDespesa,
                Categoria = despesa.Categoria,
                Observacoes = despesa.Observacoes,
                DataCriacao = despesa.DataCriacao
            };
        }
    }
}
