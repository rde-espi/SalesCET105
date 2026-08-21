using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;
using ProjetoFinalCet105.API.UseCases.Marcacoes;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarcacoesController : BaseApiController
    {
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IServicoRepository _servicoRepository;
        private readonly IFuncionarioServicoRepository _funcionarioServicoRepository;
        private readonly IHorarioFuncionarioRepository _horarioFuncionarioRepository;
        private readonly IIndisponibilidadeRepository _indisponibilidadeRepository;
        private readonly IEstadoMarcacaoRepository _estadoMarcacaoRepository;
        private readonly IHistoricoMarcacaoRepository _historicoMarcacaoRepository;
        private readonly CreateMarcacaoUseCase _createMarcacaoUseCase;
        private readonly UpdateMarcacaoUseCase _updateMarcacaoUseCase;
        private readonly UpdateEstadoMarcacaoUseCase _updateEstadoMarcacaoUseCase;
        private readonly CancelarMarcacaoUseCase _cancelarMarcacaoUseCase;
        private readonly GetDisponibilidadeUseCase _getDisponibilidadeUseCase;
        private readonly UserManager<User> _userManager;

        public MarcacoesController(IMarcacaoRepository marcacaoRepository,
                IFuncionarioRepository funcionarioRepository,
                IServicoRepository servicoRepository,
                IFuncionarioServicoRepository funcionarioServicoRepository,
                IHorarioFuncionarioRepository horarioFuncionarioRepository,
                IIndisponibilidadeRepository indisponibilidadeRepository,
                IEstadoMarcacaoRepository estadoMarcacaoRepository,
                IHistoricoMarcacaoRepository historicoMarcacaoRepository,
                CreateMarcacaoUseCase createMarcacaoUseCase,
                UpdateMarcacaoUseCase updateMarcacaoUseCase,
                UpdateEstadoMarcacaoUseCase UpdateEstadoMarcacaoUseCase,
                CancelarMarcacaoUseCase CancelarMarcacaoUseCase,
                GetDisponibilidadeUseCase GetDisponibilidadeUseCase,
                UserManager<User> userManager)
        {
            _marcacaoRepository = marcacaoRepository;
            _funcionarioRepository = funcionarioRepository;
            _servicoRepository = servicoRepository;
            _funcionarioServicoRepository = funcionarioServicoRepository;
            _horarioFuncionarioRepository = horarioFuncionarioRepository;
            _indisponibilidadeRepository = indisponibilidadeRepository;
            _estadoMarcacaoRepository = estadoMarcacaoRepository;
            _historicoMarcacaoRepository = historicoMarcacaoRepository;
            _createMarcacaoUseCase = createMarcacaoUseCase;
            _updateMarcacaoUseCase = updateMarcacaoUseCase;
            _updateEstadoMarcacaoUseCase = UpdateEstadoMarcacaoUseCase;
            _cancelarMarcacaoUseCase = CancelarMarcacaoUseCase;
            _getDisponibilidadeUseCase = GetDisponibilidadeUseCase;
            _userManager = userManager;
        }

        [Authorize(Policy ="ConsultarMarcacoes")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MarcacaoDTO>>> GetAllMarcacoes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userId == null)
            {
                return Unauthorized();
            }

            var query = _marcacaoRepository.GetAllWithDetails();

            if(User.IsInRole("Cliente") && !User.IsInRole("Admin"))
            {
                query = query.Where(m=> m.ClienteId == userId);
            }
            else if (User.IsInRole("Funcionario") && !User.IsInRole("Admin"))
            {
                var funcionario = await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

                if(funcionario == null)
                {
                    return Forbid();
                }

                query = query.Where(m => m.FuncionarioId == funcionario.Id);
            }

            var marcacoes = await query
                .OrderBy(m=> m.DataHoraInicio)
                .Select(m => new MarcacaoDTO
                {
                    Id = m.Id,

                    ClienteId = m.ClienteId,
                    ClienteNome = m.Cliente.NomeCompleto,

                    FuncionarioId = m.FuncionarioId,
                    FuncionarioNome = m.Funcionario.User.NomeCompleto,

                    ServicoId = m.ServicoId,
                    ServicoNome = m.Servico.Nome,

                    EstadoMarcacaoId = m.EstadoMarcacaoId,
                    EstadoMarcacaoNome = m.EstadoMarcacao.Nome,

                    DataHoraInicio = m.DataHoraInicio,
                    DataHoraFim = m.DataHoraFim,

                    Preco = m.Preco,
                    Observacoes = m.Observacoes,

                    DataCriacao = m.DataCriacao,
                    DataAtualizacao = m.DataAtualizacao
                })
                .ToListAsync();

            return Ok(marcacoes);
        }

        [Authorize(Policy ="ConsultarMarcacoes")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MarcacaoDTO>> GetMarcacaoById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var marcacao = await _marcacaoRepository
                .GetByIdWithDetailsAsync(id);

            if (marcacao == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Cliente") && !User.IsInRole("Admin"))
            {
                if (marcacao.ClienteId != userId)
                {
                    return Forbid();
                }
            }

            if (User.IsInRole("Funcionario") && !User.IsInRole("Admin"))
            {
                var funcionario = await _funcionarioRepository
                    .GetFuncionarioByUserIdAsync(userId);

                if (funcionario == null)
                {
                    return Forbid();
                }

                if (marcacao.FuncionarioId != funcionario.Id)
                {
                    return Forbid();
                }
            }


            return Ok(new MarcacaoDTO
            {
                Id = marcacao.Id,

                ClienteId = marcacao.ClienteId,
                ClienteNome = marcacao.Cliente.NomeCompleto,

                FuncionarioId = marcacao.FuncionarioId,
                FuncionarioNome = marcacao.Funcionario.User.NomeCompleto,

                ServicoId = marcacao.ServicoId,
                ServicoNome = marcacao.Servico.Nome,

                EstadoMarcacaoId = marcacao.EstadoMarcacaoId,
                EstadoMarcacaoNome = marcacao.EstadoMarcacao.Nome,

                DataHoraInicio = marcacao.DataHoraInicio,
                DataHoraFim = marcacao.DataHoraFim,

                Preco = marcacao.Preco,
                Observacoes = marcacao.Observacoes,

                DataCriacao = marcacao.DataCriacao,
                DataAtualizacao = marcacao.DataAtualizacao
            });
        }

        [Authorize(Policy = "CriarMarcacao")]
        [HttpPost]
        public async Task<ActionResult<MarcacaoDTO>> CreateMarcacao(NovaMarcacaoDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado = await _createMarcacaoUseCase.ExecuteAsync(
                    userId,
                    User.IsInRole("Cliente"),
                    User.IsInRole("Funcionario"),
                    User.IsInRole("Admin"),
                    dto);

            if (!resultado.Sucesso)
            {
                return BadRequest(resultado.Erro);
            }

            return CreatedAtAction(nameof(GetMarcacaoById),new { id = resultado.Dados!.Id },resultado.Dados);
        }

        [Authorize(Policy = "AlterarMarcacao")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateMarcacao(int id,UpdateMarcacaoDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado = await _updateMarcacaoUseCase.ExecuteAsync(
                    id,
                    userId,
                    User.IsInRole("Cliente"),
                    User.IsInRole("Funcionario"),
                    User.IsInRole("Admin"),
                    dto);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return NoContent();
        }

        //[HttpGet("{id:int}/estado")]
        //public async Task<ActionResult<EstadoMarcacaoDTO>> GetEstadoMarcação(int id)
        //{
        //    var marcacao = await _estadoMarcacaoRepository.GetByIdAsync(id);
        //    return Ok(new EstadoMarcacaoDTO
        //    {
        //        EstadoMarcacaoId = marcacao.Id,
        //        Nome = marcacao.Nome,
        //        Descricao = marcacao.Descricao
        //    });
        //}

        [Authorize(Policy = "GerirMarcacoes")]
        [HttpPatch("{id:int}/estado")]
        public async Task<IActionResult> UpdateEstadoMarcacao(int id,UpdateEstadoMarcacaoDTO dto)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado =
                await _updateEstadoMarcacaoUseCase.ExecuteAsync(
                    id,
                    userId,
                    User.IsInRole("Funcionario"),
                    User.IsInRole("Admin"),
                    dto);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return NoContent();

        }

        [Authorize(Policy = "CancelarMarcacao")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMarcacao(int id)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado =
                await _cancelarMarcacaoUseCase.ExecuteAsync(
                    id,
                    userId,
                    User.IsInRole("Cliente"),
                    User.IsInRole("Funcionario"),
                    User.IsInRole("Admin"));

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return NoContent();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<IEnumerable<MarcacaoDTO>>> GetMarcacoesByCliente(string clienteId)
        {
            var cliente = await _userManager.FindByIdAsync(clienteId);

            if (cliente == null ||
                !await _userManager.IsInRoleAsync(cliente, "Cliente"))
            {
                return NotFound("Cliente não encontrado.");
            }

            var marcacoes = await _marcacaoRepository
                .GetAllWithDetails()
                .Where(m => m.ClienteId == clienteId)
                .OrderBy(m => m.DataHoraInicio)
                .Select(m => new MarcacaoDTO
                {
                    Id = m.Id,

                    ClienteId = m.ClienteId,
                    ClienteNome = m.Cliente.NomeCompleto,

                    FuncionarioId = m.FuncionarioId,
                    FuncionarioNome = m.Funcionario.User.NomeCompleto,

                    ServicoId = m.ServicoId,
                    ServicoNome = m.Servico.Nome,

                    EstadoMarcacaoId = m.EstadoMarcacaoId,
                    EstadoMarcacaoNome = m.EstadoMarcacao.Nome,

                    DataHoraInicio = m.DataHoraInicio,
                    DataHoraFim = m.DataHoraFim,

                    Preco = m.Preco,
                    Observacoes = m.Observacoes,

                    DataCriacao = m.DataCriacao,
                    DataAtualizacao = m.DataAtualizacao
                })
                .ToListAsync();

            return Ok(marcacoes);
        }

        [Authorize(Policy = "ConsultarAgenda")]
        [HttpGet("funcionario/{funcionarioId:int}")]
        public async Task<ActionResult<IEnumerable<MarcacaoDTO>>> GetMarcacoesByFuncionario(int funcionarioId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            if (User.IsInRole("Funcionario") && !User.IsInRole("Admin"))
            {
                var funcionarioAutenticado =
                    await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return Forbid();
                }

                if (funcionarioAutenticado.Id != funcionarioId)
                {
                    return Forbid();
                }
            }

            if (!await _funcionarioRepository.ExistAsync(funcionarioId))
            {
                return NotFound("Funcionário não encontrado.");
            }


            var marcacoes = await _marcacaoRepository
                .GetAllWithDetails()
                .Where(m => m.FuncionarioId == funcionarioId)
                .OrderBy(m => m.DataHoraInicio)
                .Select(m => new MarcacaoDTO
                {
                    Id = m.Id,

                    ClienteId = m.ClienteId,
                    ClienteNome = m.Cliente.NomeCompleto,

                    FuncionarioId = m.FuncionarioId,
                    FuncionarioNome = m.Funcionario.User.NomeCompleto,

                    ServicoId = m.ServicoId,
                    ServicoNome = m.Servico.Nome,

                    EstadoMarcacaoId = m.EstadoMarcacaoId,
                    EstadoMarcacaoNome = m.EstadoMarcacao.Nome,

                    DataHoraInicio = m.DataHoraInicio,
                    DataHoraFim = m.DataHoraFim,

                    Preco = m.Preco,
                    Observacoes = m.Observacoes,

                    DataCriacao = m.DataCriacao,
                    DataAtualizacao = m.DataAtualizacao
                })
                .ToListAsync();

            return Ok(marcacoes);
        }

        [Authorize(Policy = "ConsultarAgenda")]
        [HttpGet("funcionario/{funcionarioId:int}/data/{data:datetime}")]
        public async Task<ActionResult<IEnumerable<MarcacaoDTO>>> GetMarcacoesByFuncionarioData(int funcionarioId, DateTime data)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            if (User.IsInRole("Funcionario") && !User.IsInRole("Admin"))
            {
                var funcionarioAutenticado =
                    await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return Forbid();
                }

                if (funcionarioAutenticado.Id != funcionarioId)
                {
                    return Forbid();
                }
            }

            if (!await _funcionarioRepository.ExistAsync(funcionarioId))
            {
                return NotFound("Funcionário não encontrado.");
            }

            var inicioDia = data.Date;
            var fimDia = inicioDia.AddDays(1);

            var marcacoes = await _marcacaoRepository
                .GetAllWithDetails()
                .Where(m =>
                    m.FuncionarioId == funcionarioId &&
                    m.DataHoraInicio >= inicioDia &&
                    m.DataHoraInicio < fimDia)
                .OrderBy(m => m.DataHoraInicio)
                .Select(m => new MarcacaoDTO
                {
                    Id = m.Id,

                    ClienteId = m.ClienteId,
                    ClienteNome = m.Cliente.NomeCompleto,

                    FuncionarioId = m.FuncionarioId,
                    FuncionarioNome = m.Funcionario.User.NomeCompleto,

                    ServicoId = m.ServicoId,
                    ServicoNome = m.Servico.Nome,

                    EstadoMarcacaoId = m.EstadoMarcacaoId,
                    EstadoMarcacaoNome = m.EstadoMarcacao.Nome,

                    DataHoraInicio = m.DataHoraInicio,
                    DataHoraFim = m.DataHoraFim,

                    Preco = m.Preco,
                    Observacoes = m.Observacoes,

                    DataCriacao = m.DataCriacao,
                    DataAtualizacao = m.DataAtualizacao
                })
                .ToListAsync();

            return Ok(marcacoes);
        }

        [HttpGet("disponibilidade")]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetDisponibilidade(int funcionarioId,int servicoId,DateTime data)
        {
            var resultado = await _getDisponibilidadeUseCase.ExecuteAsync(funcionarioId,servicoId,data);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }

        [Authorize(Policy = "ConsultarMarcacoes")]
        [HttpGet("{id:int}/historico")]
        public async Task<ActionResult<IEnumerable<HistoricoMarcacaoDTO>>> GetHistoricoMarcacao(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var marcacao = await _marcacaoRepository.GetByIdAsync(id);

            if (marcacao == null)
            {
                return NotFound("Marcação não encontrada.");
            }

            if (User.IsInRole("Cliente") && !User.IsInRole("Admin"))
            {
                if (marcacao.ClienteId != userId)
                {
                    return Forbid();
                }
            }

            if (User.IsInRole("Funcionario") && !User.IsInRole("Admin"))
            {
                var funcionarioAutenticado =
                    await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return Forbid();
                }

                if (marcacao.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return Forbid();
                }
            }

            var historico = await _historicoMarcacaoRepository
                .GetAllWithDetails()
                .Where(h => h.MarcacaoId == id)
                .OrderBy(h => h.DataAlteracao)
                .Select(h => new HistoricoMarcacaoDTO
                {
                    Id = h.Id,
                    MarcacaoId = h.MarcacaoId,
                    
                    UserId = h.UserId,
                    UserNome = h.User.NomeCompleto,
                    
                    Acao = h.Acao,
                    Descricao = h.Descricao,
                    DataAlteracao = h.DataAlteracao
                })
                .ToListAsync();
            
            return Ok(historico);
        }      
    }
}
