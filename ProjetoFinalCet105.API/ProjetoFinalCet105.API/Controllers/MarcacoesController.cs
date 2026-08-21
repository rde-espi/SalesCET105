using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarcacoesController : ControllerBase
    {
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IServicoRepository _servicoRepository;
        private readonly IFuncionarioServicoRepository _funcionarioServicoRepository;
        private readonly IHorarioFuncionarioRepository _horarioFuncionarioRepository;
        private readonly IIndisponibilidadeRepository _indisponibilidadeRepository;
        private readonly IEstadoMarcacaoRepository _estadoMarcacaoRepository;
        private readonly UserManager<User> _userManager;

        public MarcacoesController(IMarcacaoRepository marcacaoRepository,
                IFuncionarioRepository funcionarioRepository,
                IServicoRepository servicoRepository,
                IFuncionarioServicoRepository funcionarioServicoRepository,
                IHorarioFuncionarioRepository horarioFuncionarioRepository,
                IIndisponibilidadeRepository indisponibilidadeRepository,
                IEstadoMarcacaoRepository estadoMarcacaoRepository,
                UserManager<User> userManager)
        {
            _marcacaoRepository = marcacaoRepository;
            _funcionarioRepository = funcionarioRepository;
            _servicoRepository = servicoRepository;
            _funcionarioServicoRepository = funcionarioServicoRepository;
            _horarioFuncionarioRepository = horarioFuncionarioRepository;
            _indisponibilidadeRepository = indisponibilidadeRepository;
            _estadoMarcacaoRepository = estadoMarcacaoRepository;
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
            string clienteId;
            if (userId == null)
            {
                return Unauthorized();
            }

            if (User.IsInRole("Cliente"))
            {
                clienteId = userId;
            }
            else if(User.IsInRole("Funcionario") || User.IsInRole("Admin"))
            {
                if (string.IsNullOrEmpty(dto.ClienteId))
                {
                    return BadRequest("É necessário indicar o cliente da marcação");
                }
                clienteId = dto.ClienteId;
            }
            else
            {
                return Forbid();
            }            

            var cliente = await _userManager.FindByIdAsync(clienteId);
            if (cliente == null)
            {
                return BadRequest("O cliente indicado não existe");
            }        
            
            if(!await _userManager.IsInRoleAsync(cliente, "Cliente"))
            {
                return BadRequest("O utilizador indicado não é um cliente");
            }

            if (User.IsInRole("Funcionario") && !User.IsInRole("Admin"))
            {
                var funcionarioAutenticado = await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);
                if (funcionarioAutenticado == null)
                {
                    return Forbid();
                }

                if (dto.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return Forbid("Marcações que não sejam na propria agenda requer Admin access");
                }
            }

            var funcionario = await _funcionarioRepository.GetFuncionarioByIdAsync(dto.FuncionarioId);
            if (funcionario == null)
            {
                return BadRequest("O funcionário indicado não existe");
            }

            if (!funcionario.Ativo || !funcionario.Disponivel)
            {
                return BadRequest("O funcionário indicado não está disponível");
            }            

            var servico = await _servicoRepository.GetByIdAsync(dto.ServicoId);
            if(servico == null)
            {
                return BadRequest("O serviço indicado não existe");
            }
            if (!servico.Disponivel)
            {
                return BadRequest("O serviço indicado não está disponível");
            }

            var funcionarioServico = await _funcionarioServicoRepository
                .GetAll()
                .FirstOrDefaultAsync(fs =>
                fs.FuncionarioId == dto.FuncionarioId &&
                fs.ServicoId == dto.ServicoId &&
                fs.Ativo);

            if(funcionarioServico == null)
            {
                return BadRequest("O funcionário indicado não realiza este serviço");
            }

            if (dto.DataHoraInicio <= DateTime.Now)
            {
                return BadRequest("Não é possível criar uma marcação numa data/hora passada.");
            }

            var duracaoMinutos = funcionarioServico.DuracaoPersonalizadaMinutos ?? servico.DuracaoMinutos;
            
            var dataHoraFim = dto.DataHoraInicio.AddMinutes(duracaoMinutos);

            if (dataHoraFim.Date != dto.DataHoraInicio.Date)
            {
                return BadRequest("A duração do serviço ultrapassa o horário do mesmo dia.");
            }

            var preco = funcionarioServico.PrecoPersonalizado ?? servico.Preco;

            var diaSemana = dto.DataHoraInicio.DayOfWeek;

            var horarioValido = await _horarioFuncionarioRepository
                .GetAll()
                .AnyAsync(h =>
                h.FuncionarioId == dto.FuncionarioId &&
                h.DiaSemana == diaSemana &&
                h.Ativo &&
                dto.DataHoraInicio.TimeOfDay >= h.HoraInicio &&
                dataHoraFim.TimeOfDay <= h.HoraFim);

            if (!horarioValido)
            {
                return BadRequest("A marcação está fora do horário de trabalho do funcionário");
            }

            var indisponivel = await _indisponibilidadeRepository
                .GetAll()
                .AnyAsync(i =>
                i.FuncionarioId == dto.FuncionarioId &&
                dto.DataHoraInicio < i.DataHoraFim &&
                dataHoraFim > i.DataHoraInicio);
            if (indisponivel)
            {
                return BadRequest("O funcionário está indisponível neste periodo");
            }

            if (await _marcacaoRepository.ExisteSobreposicaoAsync(dto.FuncionarioId, dto.DataHoraInicio, dataHoraFim))
            {
                return BadRequest("Já existe uma marcação para este funcionário neste período");
            }

            var estadoPendente = await _estadoMarcacaoRepository
                .GetAll()
                .FirstOrDefaultAsync(e => e.Nome == "Pendente");
            if(estadoPendente == null)
            {
                return BadRequest("O estado inicial da marcação não foi encontrado");
            }

            try
            {
                var marcacao = new Marcacao
                {
                    ClienteId = clienteId,
                    FuncionarioId = dto.FuncionarioId,
                    ServicoId = dto.ServicoId,
                    EstadoMarcacaoId = estadoPendente.Id,

                    DataHoraInicio = dto.DataHoraInicio,
                    DataHoraFim = dataHoraFim,

                    Preco = preco,
                    Observacoes = dto.Observacoes,

                    DataCriacao = DateTime.Now
                };

                await _marcacaoRepository.CreateAsync(marcacao);

                var resposta = new MarcacaoDTO
                {
                    Id = marcacao.Id,

                    ClienteId = cliente.Id,
                    ClienteNome = cliente.NomeCompleto,

                    FuncionarioId = funcionario.Id,
                    FuncionarioNome = funcionario.User.NomeCompleto,

                    ServicoId = servico.Id,
                    ServicoNome = servico.Nome,

                    EstadoMarcacaoId = estadoPendente.Id,
                    EstadoMarcacaoNome = estadoPendente.Nome,

                    DataHoraInicio = marcacao.DataHoraInicio,
                    DataHoraFim = marcacao.DataHoraFim,

                    Preco = marcacao.Preco,

                    Observacoes = marcacao.Observacoes,

                    DataCriacao = marcacao.DataCriacao
                };
                return CreatedAtAction(nameof(GetMarcacaoById), new { id = marcacao.Id }, resposta);
            }
            catch (Exception)
            {
                return BadRequest();
            }
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

            var marcacaoAtual = await _marcacaoRepository.GetByIdAsync(id);
            if(marcacaoAtual == null)
            {
                return NotFound();
            }

            if(User.IsInRole("Cliente") && !User.IsInRole("Admin"))
            {
                if(marcacaoAtual.ClienteId != userId)
                {
                    return Forbid();
                }
            }

            if (User.IsInRole("Funcionario") && !User.IsInRole("Admin"))
            {
                var funcionarioAutenticado = await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);
                if (funcionarioAutenticado == null)
                {
                    return Forbid();
                }

                if(marcacaoAtual.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return Forbid();
                }

                if(dto.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return Forbid();
                }

            }

            var estadoMarcacao = await _estadoMarcacaoRepository.GetByIdAsync(marcacaoAtual.EstadoMarcacaoId);
            if(estadoMarcacao == null)
            {
                return NotFound();
            }
            if (estadoMarcacao.Nome == "Cancelada" ||
                estadoMarcacao.Nome == "Concluida" ||
                estadoMarcacao.Nome == "Não Compareceu")
            {
                return BadRequest(
                    $"Não é possível alterar uma marcação com o estado '{estadoMarcacao.Nome}'.");
            }

            var funcionario = await _funcionarioRepository.GetByIdAsync(dto.FuncionarioId);
            if(funcionario == null)
            {
                return BadRequest("O funcionário indicado não existe");
            }
            if(!funcionario.Ativo || !funcionario.Disponivel)
            {
                return BadRequest("O funcionário indicado não esta disponível");
            }

            var servico = await _servicoRepository.GetByIdAsync(dto.ServicoId);
            if(servico == null)
            {
                return BadRequest("Serviço indicado não existe");
            }
            if (!servico.Disponivel)
            {
                return BadRequest("O serviço indicado não está disponível");
            }

            var funcionarioServico = await _funcionarioServicoRepository
                .GetAll()
                .FirstOrDefaultAsync(fs => 
                fs.FuncionarioId == dto.FuncionarioId &&
                fs.ServicoId == dto.ServicoId &&
                fs.Ativo);
            if( funcionarioServico == null)
            {
                return BadRequest("O funcionário indicado não realiza este serviço");
            }

            if(dto.DataHoraInicio <= DateTime.Now)
            {
                return BadRequest("Não é possivel reagendar para uma data/hora passada");
            }

            var duracaoMinutos = funcionarioServico.DuracaoPersonalizadaMinutos ?? servico.DuracaoMinutos;

            var dataHoraFim = dto.DataHoraInicio.AddMinutes(duracaoMinutos);
            if (dataHoraFim.Date != dto.DataHoraInicio.Date)
            {
                return BadRequest("A duração do serviço ultrapassa o horário do mesmo dia");
            }

            var preco = funcionarioServico.PrecoPersonalizado ?? servico.Preco;

            var diaSemana = dto.DataHoraInicio.DayOfWeek;

            var horarioValido = await _horarioFuncionarioRepository
                .GetAll()
                .AnyAsync(h =>
                h.FuncionarioId == dto.FuncionarioId &&
                h.DiaSemana == diaSemana &&
                h.Ativo &&
                dto.DataHoraInicio.TimeOfDay >= h.HoraInicio &&
                dataHoraFim.TimeOfDay <= h.HoraFim);

            if (!horarioValido)
            {
                return BadRequest("A marcação está fora do horário de trabalho do funcionário.");
            }

            var indisponivel = await _indisponibilidadeRepository
                .GetAll()
                .AnyAsync(i =>
                i.FuncionarioId == dto.FuncionarioId &&
                dto.DataHoraInicio < i.DataHoraFim &&
                dataHoraFim > i.DataHoraInicio);
            if (indisponivel)
            {
                return BadRequest("O funcionário está indisponível neste período");
            }

            if (await _marcacaoRepository.ExisteSobreposicaoAsync(dto.FuncionarioId,dto.DataHoraInicio,dataHoraFim,id))
            {
                return BadRequest("Já existe uma marcação para este funcionário neste período.");
            }
            try
            {
                marcacaoAtual.FuncionarioId = dto.FuncionarioId;
                marcacaoAtual.ServicoId = dto.ServicoId;

                marcacaoAtual.DataHoraInicio = dto.DataHoraInicio;
                marcacaoAtual.DataHoraFim = dataHoraFim;

                marcacaoAtual.Preco = preco;
                marcacaoAtual.Observacoes = dto.Observacoes;

                marcacaoAtual.DataAtualizacao = DateTime.Now;

                await _marcacaoRepository.UpdateAsync(marcacaoAtual);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var marcacao = await _marcacaoRepository.GetByIdAsync(id);

            if (marcacao == null)
            {
                return NotFound();
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

            var estadoAtual = await _estadoMarcacaoRepository.GetByIdAsync(marcacao.EstadoMarcacaoId);

            if (estadoAtual == null)
            {
                return BadRequest("O estado atual da marcação não foi encontrado.");
            }

            var novoEstado = await _estadoMarcacaoRepository.GetByIdAsync(dto.EstadoMarcacaoId);

            if (novoEstado == null)
            {
                return BadRequest("O estado indicado não existe.");
            }

            if (estadoAtual.Nome == "Cancelada" ||
                estadoAtual.Nome == "Concluida" ||
                estadoAtual.Nome == "Não Compareceu")
            {
                return BadRequest($"A marcação encontra-se no estado '{estadoAtual.Nome}' e já não pode ser alterada.");
            }

            if (novoEstado.Nome == "Cancelada")
            {
                return BadRequest("Para cancelar uma marcação deve utilizar o endpoint de cancelamento.");
            }

            var transicaoValida =
                (estadoAtual.Nome == "Pendente" &&
                novoEstado.Nome == "Confirmada")
                ||
                (estadoAtual.Nome == "Confirmada" &&
                (novoEstado.Nome == "Concluida" ||
                novoEstado.Nome == "Não Compareceu"));

            if (!transicaoValida)
            {
                return BadRequest($"Não é possível alterar o estado de '{estadoAtual.Nome}' para '{novoEstado.Nome}'.");
            }            

            try
            {
                marcacao.EstadoMarcacaoId = novoEstado.Id;
                marcacao.DataAtualizacao = DateTime.Now;

                await _marcacaoRepository.UpdateAsync(marcacao);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [Authorize(Policy = "CancelarMarcacao")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMarcacao(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var marcacao = await _marcacaoRepository.GetByIdAsync(id);

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

            var estadoCancelada = await _estadoMarcacaoRepository
                .GetAll()
                .FirstOrDefaultAsync(e => e.Nome == "Cancelada");

            if (estadoCancelada == null)
            {
                return BadRequest("O estado Cancelada não foi encontrado.");
            }

            var estadoAtual = await _estadoMarcacaoRepository
                .GetByIdAsync(marcacao.EstadoMarcacaoId);

            if (estadoAtual == null)
            {
                return BadRequest("O estado atual da marcação não foi encontrado.");
            }

            if (estadoAtual.Nome == "Concluida" ||
                estadoAtual.Nome == "Não Compareceu")
            {
                return BadRequest(
                    $"Não é possível cancelar uma marcação com o estado '{estadoAtual.Nome}'.");
            }

            if (estadoAtual.Nome == "Cancelada")
            {
                return BadRequest("A marcação já se encontra cancelada.");
            }

            try
            {                
                marcacao.EstadoMarcacaoId = estadoCancelada.Id;
                marcacao.DataAtualizacao = DateTime.Now;

                await _marcacaoRepository.UpdateAsync(marcacao);

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest();
            }
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
            // Verificar se o funcionário existe
            var funcionario = await _funcionarioRepository
                .GetByIdAsync(funcionarioId);

            if (funcionario == null)
            {
                return NotFound("Funcionário não encontrado.");
            }

            if (!funcionario.Ativo || !funcionario.Disponivel)
            {
                return BadRequest("O funcionário não está disponível.");
            }

            // Verificar se o serviço existe
            var servico = await _servicoRepository
                .GetByIdAsync(servicoId);

            if (servico == null)
            {
                return NotFound("Serviço não encontrado.");
            }

            if (!servico.Disponivel)
            {
                return BadRequest("O serviço não está disponível.");
            }

            // Verificar se o funcionário realiza o serviço
            var funcionarioServico = await _funcionarioServicoRepository
                .GetAll()
                .FirstOrDefaultAsync(fs =>
                    fs.FuncionarioId == funcionarioId &&
                    fs.ServicoId == servicoId &&
                    fs.Ativo);

            if (funcionarioServico == null)
            {
                return BadRequest(
                    "O funcionário indicado não realiza este serviço.");
            }

            // Descobrir a duração real do serviço para este funcionário
            var duracaoMinutos =
                funcionarioServico.DuracaoPersonalizadaMinutos
                ?? servico.DuracaoMinutos;

            // Procurar o horário de trabalho desse funcionário nesse dia
            var diaSemana = data.DayOfWeek;

            var horariosTrabalho = await _horarioFuncionarioRepository
                .GetAll()
                .Where(h =>
                    h.FuncionarioId == funcionarioId &&
                    h.DiaSemana == diaSemana &&
                    h.Ativo)
                .ToListAsync();

            // Se não trabalha nesse dia, não há horários disponíveis
            if (!horariosTrabalho.Any())
            {
                return Ok(new List<DateTime>());
            }

            // Intervalo completo do dia
            var inicioDia = data.Date;
            var fimDia = inicioDia.AddDays(1);

            // Buscar indisponibilidades desse funcionário nesse dia
            var indisponibilidades = await _indisponibilidadeRepository
                .GetAll()
                .Where(i =>
                    i.FuncionarioId == funcionarioId &&
                    i.DataHoraInicio < fimDia &&
                    i.DataHoraFim > inicioDia)
                .ToListAsync();

            // Buscar marcações existentes desse funcionário nesse dia.
            var marcacoes = await _marcacaoRepository
                .GetAllWithDetails()
                .Where(m =>
                    m.FuncionarioId == funcionarioId &&
                    m.DataHoraInicio < fimDia &&
                    m.DataHoraFim > inicioDia &&
                    m.EstadoMarcacao.Nome != "Cancelada")
                .ToListAsync();

            var horariosDisponiveis = new List<DateTime>();

            
            foreach (var horarioTrabalho in horariosTrabalho)
            {
                var inicioTrabalho =
                    data.Date.Add(horarioTrabalho.HoraInicio);

                var fimTrabalho =
                    data.Date.Add(horarioTrabalho.HoraFim);

                var horaAtual = inicioTrabalho;

                while (horaAtual.AddMinutes(duracaoMinutos) <= fimTrabalho)
                {
                    var horaFimServico =
                        horaAtual.AddMinutes(duracaoMinutos);

                    // Não mostrar horários que já passaram
                    var horarioNoPassado = horaAtual <= DateTime.Now;

                    // Verificar se existe alguma indisponibilidade que colida com este horário
                    var temIndisponibilidade = indisponibilidades.Any(i =>
                        horaAtual < i.DataHoraFim &&
                        horaFimServico > i.DataHoraInicio);

                    // Verificar se existe uma marcação que colida com este horário
                    var temMarcacao = marcacoes.Any(m =>
                        horaAtual < m.DataHoraFim &&
                        horaFimServico > m.DataHoraInicio);

                    if (!horarioNoPassado &&
                        !temIndisponibilidade &&
                        !temMarcacao)
                    {
                        horariosDisponiveis.Add(horaAtual);
                    }

                    // Próxima possibilidade de início
                    horaAtual = horaAtual.AddMinutes(30);
                }
            }

            return Ok(horariosDisponiveis
                .Distinct()
                .OrderBy(h => h));
        }
    }
}
