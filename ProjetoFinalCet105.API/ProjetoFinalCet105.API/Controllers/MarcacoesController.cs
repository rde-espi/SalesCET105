using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MarcacaoDTO>>> GetAllMarcacoes()
        {
            var marcacoes = await _marcacaoRepository
                .GetAllWithDetails()
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

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MarcacaoDTO>> GetMarcacaoById(int id)
        {
            var marcacao = await _marcacaoRepository
                .GetByIdWithDetailsAsync(id);

            if (marcacao == null)
            {
                return NotFound();
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

        [HttpPost]
        public async Task<ActionResult<MarcacaoDTO>> CreateMarcacao(NovaMarcacaoDTO dto)
        {

            var cliente = await _userManager.FindByIdAsync(dto.ClienteId);
            if (cliente == null)
            {
                return BadRequest("O cliente indicado não existe");
            }

            if (!await _userManager.IsInRoleAsync(cliente, "Cliente"))
            {
                return BadRequest("O utilizador indicado não é um cliente.");
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
                    ClienteId = dto.ClienteId,
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

    }
}
