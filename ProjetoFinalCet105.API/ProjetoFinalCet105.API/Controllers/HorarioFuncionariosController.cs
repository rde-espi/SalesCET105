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
    public class HorarioFuncionariosController : ControllerBase
    {
        private readonly IHorarioFuncionarioRepository _horarioFuncionarioRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;

        public HorarioFuncionariosController(IHorarioFuncionarioRepository horarioFuncionarioRepository,IFuncionarioRepository funcionarioRepository)
        {
            _horarioFuncionarioRepository = horarioFuncionarioRepository;
            _funcionarioRepository = funcionarioRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HorarioFuncionarioDTO>>> GetAllHorariosFuncionarios()
        {
            var horarios = await _horarioFuncionarioRepository.GetAllWithFuncionario()
                .Select(hf=> new HorarioFuncionarioDTO
                {
                    Id = hf.Id,
                    FuncionarioId = hf.FuncionarioId,
                    FuncionarioNome = hf.Funcionario.User.NomeCompleto,
                    DiaSemana = hf.DiaSemana,
                    HoraInicio = hf.HoraInicio,
                    HoraFim = hf.HoraFim,
                    Ativo = hf.Ativo
                })
                .ToListAsync();
            return Ok(horarios);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<HorarioFuncionarioDTO>>GetHorarioFuncionarioById(int id)
        {
            var horario = await _horarioFuncionarioRepository.GetByIdWithFuncionarioAsync(id);
            if(horario == null)
            {
                return NotFound();
            }
            return Ok(new HorarioFuncionarioDTO
            {
                Id = horario.Id,
                FuncionarioId=horario.FuncionarioId,
                FuncionarioNome = horario.Funcionario.User.NomeCompleto,
                DiaSemana=horario.DiaSemana,
                HoraInicio=horario.HoraInicio,
                HoraFim=horario.HoraFim,
                Ativo=horario.Ativo
            });
        }

        [HttpPost]
        public async Task<ActionResult<HorarioFuncionarioDTO>>CreateHorarioFuncionario(HorarioFuncionarioDTO dto)
        {
            if (!await _funcionarioRepository.ExistAsync(dto.FuncionarioId))
            {
                return BadRequest("O funcionário indicado não existe.");
            }

            if (dto.HoraFim <= dto.HoraInicio)
            {
                return BadRequest(
                    "A hora de fim deve ser posterior à hora de início.");
            }

            if (await _horarioFuncionarioRepository.ExisteSobreposicaoAsync(dto.FuncionarioId,dto.DiaSemana,dto.HoraInicio,dto.HoraFim))
            {
                return BadRequest(
                    "Já existe um horário sobreposto para este funcionário nesse dia.");
            }

            try
            {
                var horario = new HorarioFuncionario
                {
                    FuncionarioId = dto.FuncionarioId,
                    DiaSemana = dto.DiaSemana,
                    HoraInicio = dto.HoraInicio,
                    HoraFim = dto.HoraFim,
                    Ativo = dto.Ativo
                };

                await _horarioFuncionarioRepository.CreateAsync(horario);

                dto.Id = horario.Id;

                var funcionario = await _funcionarioRepository
                    .GetFuncionarioByIdAsync(dto.FuncionarioId);

                dto.FuncionarioNome = funcionario!.User.NomeCompleto;

                return CreatedAtAction(
                    nameof(GetHorarioFuncionarioById),
                    new { id = horario.Id },
                    dto);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateHorarioFuncionario(int id,HorarioFuncionarioDTO dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            if (!await _horarioFuncionarioRepository.ExistAsync(id))
            {
                return NotFound();
            }

            if (!await _funcionarioRepository.ExistAsync(dto.FuncionarioId))
            {
                return BadRequest("O funcionário indicado não existe.");
            }

            if (dto.HoraFim <= dto.HoraInicio)
            {
                return BadRequest(
                    "A hora de fim deve ser posterior à hora de início.");
            }

            if (await _horarioFuncionarioRepository.ExisteSobreposicaoAsync(dto.FuncionarioId,dto.DiaSemana,dto.HoraInicio,dto.HoraFim,id))
            {
                return BadRequest(
                    "Já existe um horário sobreposto para este funcionário nesse dia.");
            }

            try
            {
                var horario = new HorarioFuncionario
                {
                    Id = id,
                    FuncionarioId = dto.FuncionarioId,
                    DiaSemana = dto.DiaSemana,
                    HoraInicio = dto.HoraInicio,
                    HoraFim = dto.HoraFim,
                    Ativo = dto.Ativo
                };

                await _horarioFuncionarioRepository.UpdateAsync(horario);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }



        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHorarioFuncionario(int id)
        {
            var horario = await _horarioFuncionarioRepository.GetByIdAsync(id);

            if (horario == null)
            {
                return NotFound();
            }

            try
            {
                horario.Ativo = false;

                await _horarioFuncionarioRepository.UpdateAsync(horario);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
