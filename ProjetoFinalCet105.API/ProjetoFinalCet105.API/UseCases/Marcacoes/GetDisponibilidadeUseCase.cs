using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services;
using ProjetoFinalCet105.API.Services.MarcacaoService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Marcacoes
{
    public class GetDisponibilidadeUseCase
    {
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IServicoRepository _servicoRepository;
        private readonly IHorarioFuncionarioRepository _horarioFuncionarioRepository;
        private readonly IIndisponibilidadeRepository _indisponibilidadeRepository;
        private readonly IMarcacaoService _marcacaoService;
        private readonly IMarcacaoRepository _marcacaoRepository;

        public GetDisponibilidadeUseCase(
            IFuncionarioRepository funcionarioRepository,
            IServicoRepository servicoRepository,
            IHorarioFuncionarioRepository horarioFuncionarioRepository,
            IIndisponibilidadeRepository indisponibilidadeRepository,
            IMarcacaoService marcacaoService,
            IMarcacaoRepository marcacaoRepository)
        {
            _funcionarioRepository = funcionarioRepository;
            _servicoRepository = servicoRepository;
            _horarioFuncionarioRepository = horarioFuncionarioRepository;
            _indisponibilidadeRepository = indisponibilidadeRepository;
            _marcacaoService = marcacaoService;
            _marcacaoRepository = marcacaoRepository;
        }

        public async Task<UseCaseResult<IEnumerable<DateTime>>> ExecuteAsync(int funcionarioId,int servicoId,DateTime data)
        {
            var funcionario = await _funcionarioRepository.GetByIdAsync(funcionarioId);

            if (funcionario == null)
            {
                return UseCaseResult<IEnumerable<DateTime>>
                    .Falha("Funcionário não encontrado.", TipoErro.NaoEncontrado);
            }

            if (!funcionario.Ativo || !funcionario.Disponivel)
            {
                return UseCaseResult<IEnumerable<DateTime>>
                    .Falha("O funcionário não está disponível.");
            }

            var servico = await _servicoRepository
                .GetByIdAsync(servicoId);

            if (servico == null)
            {
                return UseCaseResult<IEnumerable<DateTime>>
                    .Falha("Serviço não encontrado.", TipoErro.NaoEncontrado);
            }

            if (!servico.Disponivel)
            {
                return UseCaseResult<IEnumerable<DateTime>>
                    .Falha("O serviço não está disponível.");
            }

            var funcionarioServico = await _marcacaoService.GetFuncionarioServicoAsync(funcionario.Id, servico.Id);

            if (funcionarioServico == null)
            {
                return UseCaseResult<IEnumerable<DateTime>>
                    .Falha("O funcionário indicado não realiza este serviço.");
            }

            var duracaoMinutos =
                funcionarioServico.DuracaoPersonalizadaMinutos
                ?? servico.DuracaoMinutos;

            var diaSemana = data.DayOfWeek;

            var horariosTrabalho = await _horarioFuncionarioRepository
                .GetAll()
                .Where(h =>
                    h.FuncionarioId == funcionarioId &&
                    h.DiaSemana == diaSemana &&
                    h.Ativo)
                .ToListAsync();

            if (!horariosTrabalho.Any())
            {
                return UseCaseResult<IEnumerable<DateTime>>
                    .Ok(new List<DateTime>());
            }

            var inicioDia = data.Date;
            var fimDia = inicioDia.AddDays(1);

            var indisponibilidades = await _indisponibilidadeRepository
                .GetAll()
                .Where(i =>
                    i.FuncionarioId == funcionarioId &&
                    i.DataHoraInicio < fimDia &&
                    i.DataHoraFim > inicioDia)
                .ToListAsync();

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

                    var horarioNoPassado =
                        horaAtual <= DateTime.Now;

                    var temIndisponibilidade =
                        indisponibilidades.Any(i =>
                            horaAtual < i.DataHoraFim &&
                            horaFimServico > i.DataHoraInicio);

                    var temMarcacao =
                        marcacoes.Any(m =>
                            horaAtual < m.DataHoraFim &&
                            horaFimServico > m.DataHoraInicio);

                    if (!horarioNoPassado &&
                        !temIndisponibilidade &&
                        !temMarcacao)
                    {
                        horariosDisponiveis.Add(horaAtual);
                    }

                    horaAtual = horaAtual.AddMinutes(30);
                }
            }

            var resultado = horariosDisponiveis
                .Distinct()
                .OrderBy(h => h)
                .ToList();

            return UseCaseResult<IEnumerable<DateTime>>
                .Ok(resultado);
        }
    }
}
