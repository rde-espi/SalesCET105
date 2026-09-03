using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.DashboardService;

namespace ProjetoFinalCet105.API.UseCases.Dashboard
{
    public class GetDashboardAgendaUseCase
    {
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IOcupacaoAgendaService _ocupacaoAgendaService;

        public GetDashboardAgendaUseCase(IMarcacaoRepository marcacaoRepository, IOcupacaoAgendaService ocupacaoAgendaService)
        {
            _marcacaoRepository = marcacaoRepository;
            _ocupacaoAgendaService = ocupacaoAgendaService;
        }

        public async Task<DashboardAgendaDTO> ExecuteAsync()
        {
            var hoje = DateTime.Today;

            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

            var inicioProximoMes = inicioMes.AddMonths(1);

            var ocupacao = await _ocupacaoAgendaService.CalcularAsync(inicioMes,inicioProximoMes);

            var taxaOcupacao =
                ocupacao.HorasDisponiveis > 0
                    ? Math.Round(
                        ocupacao.HorasOcupadas /
                        ocupacao.HorasDisponiveis * 100,
                        2)
                    : 0;

            var marcacoesMes = _marcacaoRepository
                .GetAllWithDetails()
                .Where(m =>
                    m.DataHoraInicio >= inicioMes &&
                    m.DataHoraInicio < inicioProximoMes);

            var totalMarcacoesMes = await marcacoesMes.CountAsync();

            var concluidas = await marcacoesMes.CountAsync(m =>m.EstadoMarcacao.Nome == "Concluida");

            var canceladas = await marcacoesMes.CountAsync(m => m.EstadoMarcacao.Nome == "Cancelada");

            var naoCompareceu = await marcacoesMes.CountAsync(m => m.EstadoMarcacao.Nome == "Não Compareceu");

            var pendentes = await marcacoesMes.CountAsync(m => m.EstadoMarcacao.Nome == "Pendente");

            var confirmadas = await marcacoesMes.CountAsync(m => m.EstadoMarcacao.Nome == "Confirmada");

            decimal CalcularTaxa(int quantidade)
            {
                if (totalMarcacoesMes == 0)
                    return 0;

                return Math.Round((decimal)quantidade / totalMarcacoesMes * 100,2);
            }

            return new DashboardAgendaDTO
            {
                TotalMarcacoesMes = totalMarcacoesMes,

                MarcacoesConcluidasMes = concluidas,
                MarcacoesCanceladasMes = canceladas,
                NaoCompareceuMes = naoCompareceu,
                MarcacoesPendentesMes = pendentes,
                MarcacoesConfirmadasMes = confirmadas,

                HorasDisponiveisMes = ocupacao.HorasDisponiveis,
                HorasOcupadasMes = ocupacao.HorasOcupadas,
                TaxaOcupacao = taxaOcupacao,
                HorasLivresMes = Math.Round(Math.Max(0,ocupacao.HorasDisponiveis -ocupacao.HorasOcupadas),2),

                TaxaConclusao = CalcularTaxa(concluidas),
                TaxaCancelamento = CalcularTaxa(canceladas),
                TaxaNaoComparecimento = CalcularTaxa(naoCompareceu)
            };
        }

        public async Task<List<HorarioMaiorProcuraDTO>>ExecuteHorariosMaiorProcuraAsync(int limite = 5)
        {
            var hoje = DateTime.Today;

            var inicioMes = new DateTime( hoje.Year, hoje.Month, 1);

            var inicioProximoMes = inicioMes.AddMonths(1);

            var marcacoes = _marcacaoRepository
                .GetAllWithDetails()
                .Where(m =>
                    m.DataHoraInicio >= inicioMes &&
                    m.DataHoraInicio < inicioProximoMes &&
                    m.EstadoMarcacao.Nome != "Cancelada");

            var totalMarcacoes = await marcacoes.CountAsync();

            if (totalMarcacoes == 0)
                return new List<HorarioMaiorProcuraDTO>();

            var resultado = await marcacoes
                .GroupBy(m => m.DataHoraInicio.Hour)
                .Select(g => new
                {
                    Hora = g.Key,
                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Quantidade)
                .ThenBy(x => x.Hora)
                .Take(limite)
                .ToListAsync();

            return resultado
                .Select(x => new HorarioMaiorProcuraDTO
                {
                    Hora = x.Hora,

                    FaixaHoraria = $"{x.Hora:00}:00 - {(x.Hora + 1):00}:00",

                    QuantidadeMarcacoes = x.Quantidade,

                    Percentagem = Math.Round(
                        (decimal)x.Quantidade /
                        totalMarcacoes * 100,
                        2)
                })
                .ToList();
        }

        public async Task<List<ServicoMaisMarcadoDTO>>ExecuteServicosMaisMarcadosAsync(int limite = 5)
        {
            var hoje = DateTime.Today;

            var inicioMes = new DateTime( hoje.Year, hoje.Month, 1);

            var inicioProximoMes = inicioMes.AddMonths(1);

            var marcacoes = _marcacaoRepository
                .GetAllWithDetails()
                .Where(m =>
                    m.DataHoraInicio >= inicioMes &&
                    m.DataHoraInicio < inicioProximoMes &&
                    m.EstadoMarcacao.Nome != "Cancelada");

            var totalMarcacoes = await marcacoes.CountAsync();

            if (totalMarcacoes == 0)
                return new List<ServicoMaisMarcadoDTO>();

            var resultado = await marcacoes
                .GroupBy(m => new
                {
                    m.ServicoId,
                    NomeServico = m.Servico.Nome
                })
                .Select(g => new
                {
                    ServicoId = g.Key.ServicoId,
                    NomeServico = g.Key.NomeServico,
                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Quantidade)
                .ThenBy(x => x.NomeServico)
                .Take(limite)
                .ToListAsync();

            return resultado
                .Select(x => new ServicoMaisMarcadoDTO
                {
                    ServicoId = x.ServicoId,
                    NomeServico = x.NomeServico,
                    QuantidadeMarcacoes = x.Quantidade,

                    Percentagem = Math.Round(
                        (decimal)x.Quantidade /
                        totalMarcacoes * 100,
                        2)
                })
                .ToList();
        }

        public async Task<List<DiaSemanaProcuraDTO>>ExecuteDiasMaiorProcuraAsync()
        {
            var hoje = DateTime.Today;

            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

            var inicioProximoMes = inicioMes.AddMonths(1);

            var marcacoes = await _marcacaoRepository
                .GetAllWithDetails()
                .Where(m =>
                    m.DataHoraInicio >= inicioMes &&
                    m.DataHoraInicio < inicioProximoMes &&
                    m.EstadoMarcacao.Nome != "Cancelada")
                .Select(m => m.DataHoraInicio)
                .ToListAsync();

            var totalMarcacoes = marcacoes.Count;

            if (totalMarcacoes == 0)
                return new List<DiaSemanaProcuraDTO>();

            return marcacoes
                .GroupBy(data => data.DayOfWeek)
                .Select(g => new DiaSemanaProcuraDTO
                {
                    DiaSemana = (int)g.Key,

                    NomeDia = ObterNomeDiaSemana(g.Key),

                    QuantidadeMarcacoes = g.Count(),

                    Percentagem = Math.Round(
                        (decimal)g.Count() /
                        totalMarcacoes * 100,
                        2)
                })
                .OrderByDescending(x => x.QuantidadeMarcacoes)
                .ThenBy(x => x.DiaSemana)
                .ToList();
        }

        private static string ObterNomeDiaSemana(DayOfWeek dia)
        {
            return dia switch
            {
                DayOfWeek.Monday => "Segunda-feira",
                DayOfWeek.Tuesday => "Terça-feira",
                DayOfWeek.Wednesday => "Quarta-feira",
                DayOfWeek.Thursday => "Quinta-feira",
                DayOfWeek.Friday => "Sexta-feira",
                DayOfWeek.Saturday => "Sábado",
                DayOfWeek.Sunday => "Domingo",
                _ => string.Empty
            };
        }
    }
}
