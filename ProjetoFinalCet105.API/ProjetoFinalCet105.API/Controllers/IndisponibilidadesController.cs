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
    public class IndisponibilidadesController : ControllerBase
    {
        private readonly IIndisponibilidadeRepository _indisponibilidadeRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;

        public IndisponibilidadesController(IIndisponibilidadeRepository indisponibilidadeRepository,IFuncionarioRepository funcionarioRepository)
        {
            _indisponibilidadeRepository = indisponibilidadeRepository;
            _funcionarioRepository = funcionarioRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<IndisponibilidadeDTO>>> GetAllIndisponibilidadesWithFuncionario()
        {
            var indisponibilidades = await _indisponibilidadeRepository.GetAllIndisponibilidadesWithFuncionario()
                .Select(i=> new IndisponibilidadeDTO
                {
                    Id = i.Id,
                    FuncionarioId = i.FuncionarioId,
                    FuncionarioNome = i.Funcionario.User.NomeCompleto,
                    DataHoraInicio = i.DataHoraInicio,
                    DataHoraFim = i.DataHoraFim,
                    Motivo = i.Motivo,
                    DiaCompleto = i.DiaCompleto
                })
                .ToListAsync();
            return Ok(indisponibilidades);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<IndisponibilidadeDTO>>GetIndisponibilidadeWithFuncionarioById(int id)
        {
            var indisponibilidade = await _indisponibilidadeRepository.GetIndisponibilidadeWithFuncionarioByIdAsync(id);
            if(indisponibilidade == null)
            {
                return NotFound();
            }
            return Ok(new IndisponibilidadeDTO
            {
                Id = indisponibilidade.Id,
                FuncionarioId = indisponibilidade.FuncionarioId,
                FuncionarioNome = indisponibilidade.Funcionario.User.NomeCompleto,
                DataHoraInicio = indisponibilidade.DataHoraInicio,
                DataHoraFim = indisponibilidade.DataHoraFim,
                Motivo = indisponibilidade.Motivo,
                DiaCompleto = indisponibilidade.DiaCompleto
            });
        }

        [HttpPost]
        public async Task<ActionResult<IndisponibilidadeDTO>>CreateIndisponibilidade(IndisponibilidadeDTO dto)
        {
            if (!await _funcionarioRepository.ExistAsync(dto.FuncionarioId))
            {
                return BadRequest("O funcionário indicado não existe.");
            }

            if (dto.DataHoraFim <= dto.DataHoraInicio)
            {
                return BadRequest(
                    "A data/hora de fim deve ser posterior à data/hora de início.");
            }

            if (await _indisponibilidadeRepository.ExisteSobreposiçãoAsync(dto.FuncionarioId,dto.DataHoraInicio,dto.DataHoraFim))
            {
                return BadRequest(
                    "Já existe uma indisponibilidade sobreposta para este funcionário.");
            }

            try
            {
                var indisponibilidade = new Indisponibilidade
                {
                    FuncionarioId = dto.FuncionarioId,
                    DataHoraInicio = dto.DataHoraInicio,
                    DataHoraFim = dto.DataHoraFim,
                    Motivo = dto.Motivo,
                    DiaCompleto = dto.DiaCompleto,
                };

                await _indisponibilidadeRepository.CreateAsync(indisponibilidade);

                dto.Id = indisponibilidade.Id;

                var funcionario = await _funcionarioRepository.GetFuncionarioByIdAsync(dto.FuncionarioId);

                dto.FuncionarioNome = funcionario!.User.NomeCompleto;

                return CreatedAtAction(nameof(GetIndisponibilidadeWithFuncionarioById),new { id = indisponibilidade.Id },dto);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateIndisponibilidade(int id, IndisponibilidadeDTO dto)
        {
            if (!await _funcionarioRepository.ExistAsync(dto.FuncionarioId))
            {
                return BadRequest("O funcionário indicado não existe.");
            }
            if (id != dto.Id)
            {
                return BadRequest();
            }

            if(!await _indisponibilidadeRepository.ExistAsync(id))
            {
                return NotFound();
            }
            if(dto.DataHoraFim <= dto.DataHoraInicio)
            {
                return BadRequest("A data/hora de fim deve ser posterior à data/hora de ínicio");
            }
            if(await _indisponibilidadeRepository.ExisteSobreposiçãoAsync(dto.FuncionarioId, dto.DataHoraInicio, dto.DataHoraFim, id))
            {
                return BadRequest("Já existe uma indisponibilidade sobreposta para este funcionário");
            }

            try
            {
                var indisponibilidade = new Indisponibilidade
                {
                    Id = id,
                    FuncionarioId = dto.FuncionarioId,
                    DataHoraInicio = dto.DataHoraInicio,
                    DataHoraFim = dto.DataHoraFim,
                    Motivo = dto.Motivo,
                    DiaCompleto = dto.DiaCompleto
                };

                await _indisponibilidadeRepository.UpdateAsync(indisponibilidade);

                return NoContent();
            }
            catch (Exception)
            {

                return BadRequest();
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult>DeleteIndisponibilidade(int id)
        {
            var indisponibilidade = await _indisponibilidadeRepository.GetByIdAsync(id);
            if(indisponibilidade == null)
            {
                return NotFound();
            }
            try
            {
                await _indisponibilidadeRepository.DeleteAsync(indisponibilidade);
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
