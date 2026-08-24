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
    public class FuncionarioServicosController : ControllerBase
    {
        private readonly IFuncionarioServicoRepository _funcionarioServicoRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IServicoRepository _servicoRepository;

        public FuncionarioServicosController(IFuncionarioServicoRepository funcionarioServicoRepository, IFuncionarioRepository funcionarioRepository, IServicoRepository servicoRepository)
        {
            _funcionarioServicoRepository = funcionarioServicoRepository;
            _funcionarioRepository = funcionarioRepository;
            _servicoRepository = servicoRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FuncionarioServicoDTO>>> GetAllFuncionarioServicos()
        {
            var funcionarioServicos = await _funcionarioServicoRepository
                .GetAllWithDetails()
                .Select(fs => new FuncionarioServicoDTO
                {
                    Id = fs.Id,

                    FuncionarioId = fs.FuncionarioId,
                    FuncionarioNome = fs.Funcionario.User.NomeCompleto,

                    ServicoId = fs.ServicoId,
                    ServicoNome = fs.Servico.Nome,

                    PrecoPersonalizado = fs.PrecoPersonalizado,
                    DuracaoPersonalizadaMinutos = fs.DuracaoPersonalizadaMinutos,

                    Ativo = fs.Ativo
                })
                .ToListAsync();

            return Ok(funcionarioServicos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<FuncionarioServicoDTO>> GetFuncionarioServicoById(int id)
        {
            var funcionarioServico = await _funcionarioServicoRepository
                .GetByIdWithDetailsAsync(id);

            if (funcionarioServico == null)
            {
                return NotFound();
            }

            return Ok(new FuncionarioServicoDTO
            {
                Id = funcionarioServico.Id,

                FuncionarioId = funcionarioServico.FuncionarioId,
                FuncionarioNome = funcionarioServico.Funcionario.User.NomeCompleto,

                ServicoId = funcionarioServico.ServicoId,
                ServicoNome = funcionarioServico.Servico.Nome,

                PrecoPersonalizado = funcionarioServico.PrecoPersonalizado,
                DuracaoPersonalizadaMinutos =
                    funcionarioServico.DuracaoPersonalizadaMinutos,

                Ativo = funcionarioServico.Ativo
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<FuncionarioServicoDTO>> CreateFuncionarioServico(FuncionarioServicoDTO dto)
        {
            if (!await _funcionarioRepository.ExistAsync(dto.FuncionarioId))
            {
                return BadRequest("O funcionário indicado não existe.");
            }

            if (!await _servicoRepository.ExistAsync(dto.ServicoId))
            {
                return BadRequest("O serviço indicado não existe.");
            }

            if (await _funcionarioServicoRepository.ExistFuncionarioServicoAsync(dto.FuncionarioId, dto.ServicoId))
            {
                return BadRequest(
                    "Este funcionário já está associado a este serviço.");
            }

            try
            {
                var funcionarioServico = new FuncionarioServico
                {
                    FuncionarioId = dto.FuncionarioId,
                    ServicoId = dto.ServicoId,
                    PrecoPersonalizado = dto.PrecoPersonalizado,
                    DuracaoPersonalizadaMinutos =
                        dto.DuracaoPersonalizadaMinutos,
                    Ativo = true
                };

                await _funcionarioServicoRepository
                    .CreateAsync(funcionarioServico);

                dto.Id = funcionarioServico.Id;

                var funcionario = await _funcionarioRepository
                    .GetFuncionarioByIdAsync(dto.FuncionarioId);

                var servico = await _servicoRepository
                    .GetByIdAsync(dto.ServicoId);

                dto.FuncionarioNome = funcionario!.User.NomeCompleto;
                dto.ServicoNome = servico!.Nome;

                return CreatedAtAction(
                    nameof(GetFuncionarioServicoById),
                    new { id = funcionarioServico.Id },
                    dto);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFuncionarioServico(int id, FuncionarioServicoDTO dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            if (!await _funcionarioServicoRepository.ExistAsync(id))
            {
                return NotFound();
            }

            if (!await _funcionarioRepository.ExistAsync(dto.FuncionarioId))
            {
                return BadRequest("O funcionário indicado não existe.");
            }

            if (!await _servicoRepository.ExistAsync(dto.ServicoId))
            {
                return BadRequest("O serviço indicado não existe.");
            }
            if (await _funcionarioServicoRepository.ExistFuncionarioServicoAsync(dto.FuncionarioId, dto.ServicoId, id))
            {
                return BadRequest(
                    "Este funcionário já está associado a este serviço.");
            }

            try
            {
                var funcionarioServico = new FuncionarioServico
                {
                    Id = id,
                    FuncionarioId = dto.FuncionarioId,
                    ServicoId = dto.ServicoId,
                    PrecoPersonalizado = dto.PrecoPersonalizado,
                    DuracaoPersonalizadaMinutos =
                        dto.DuracaoPersonalizadaMinutos,
                    Ativo = dto.Ativo
                };

                await _funcionarioServicoRepository
                    .UpdateAsync(funcionarioServico);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFuncionarioServico(int id)
        {
            var funcionarioServico = await _funcionarioServicoRepository
                .GetByIdAsync(id);

            if (funcionarioServico == null)
            {
                return NotFound();
            }

            try
            {
                funcionarioServico.Ativo = false;

                await _funcionarioServicoRepository
                    .UpdateAsync(funcionarioServico);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
