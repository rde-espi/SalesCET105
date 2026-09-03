using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.Services.DashboardService
{
    public class OcupacaoAgendaService : IOcupacaoAgendaService
    {
        private readonly IHorarioFuncionarioRepository _horarioRepository;
        private readonly IIndisponibilidadeRepository _indisponibilidadeRepository;
        private readonly IMarcacaoRepository _marcacaoRepository;

        public OcupacaoAgendaService(
            IHorarioFuncionarioRepository horarioRepository,
            IIndisponibilidadeRepository indisponibilidadeRepository,
            IMarcacaoRepository marcacaoRepository)
        {
            _horarioRepository = horarioRepository;
            _indisponibilidadeRepository = indisponibilidadeRepository;
            _marcacaoRepository = marcacaoRepository;
        }

        public async Task<(decimal HorasDisponiveis, decimal HorasOcupadas)>CalcularAsync( DateTime dataInicio, DateTime dataFim)
        {
            var horarios = await _horarioRepository
                .GetAllWithFuncionario()
                .Where(h => h.Ativo)
                .ToListAsync();

            var indisponibilidades = await _indisponibilidadeRepository
                .GetAllIndisponibilidadesWithFuncionario()
                .Where(i =>
                    i.DataHoraInicio < dataFim &&
                    i.DataHoraFim > dataInicio)
                .ToListAsync();

            var marcacoes = await _marcacaoRepository
                .GetAllWithDetails()
                .Where(m =>
                    m.DataHoraInicio < dataFim &&
                    m.DataHoraFim > dataInicio &&
                    m.EstadoMarcacao.Nome != "Cancelada")
                .ToListAsync();

            decimal minutosDisponiveis = 0;
            decimal minutosOcupados = 0;

            for (var dia = dataInicio.Date;
                 dia < dataFim.Date;
                 dia = dia.AddDays(1))
            {
                var horariosDia = horarios
                    .Where(h => h.DiaSemana == dia.DayOfWeek)
                    .ToList();

                foreach (var horario in horariosDia)
                {
                    var inicioHorario = dia.Add(horario.HoraInicio);
                    var fimHorario = dia.Add(horario.HoraFim);

                    var minutosHorario =
                        (decimal)(fimHorario - inicioHorario).TotalMinutes;

                    var minutosIndisponiveis =
                        CalcularMinutosSobrepostos(
                            inicioHorario,
                            fimHorario,
                            indisponibilidades
                                .Where(i =>
                                    i.FuncionarioId == horario.FuncionarioId)
                                .Select(i => (
                                    Inicio: i.DataHoraInicio,
                                    Fim: i.DataHoraFim)));

                    var disponivel = Math.Max(0, minutosHorario - minutosIndisponiveis);

                    minutosDisponiveis += disponivel;

                    var minutosMarcacoes =
                        CalcularMinutosSobrepostos(
                            inicioHorario,
                            fimHorario,
                            marcacoes
                                .Where(m =>
                                    m.FuncionarioId == horario.FuncionarioId)
                                .Select(m => (
                                    Inicio: m.DataHoraInicio,
                                    Fim: m.DataHoraFim)));

                    minutosOcupados += Math.Min(minutosMarcacoes, disponivel);
                }
            }

            return ( HorasDisponiveis:Math.Round(minutosDisponiveis / 60, 2),HorasOcupadas: Math.Round(minutosOcupados / 60, 2));
        }

        private static decimal CalcularMinutosSobrepostos(DateTime inicioLimite, DateTime fimLimite, IEnumerable<(DateTime Inicio, DateTime Fim)> periodos)
        {
            var intervalos = periodos
                .Where(p =>
                    p.Inicio < fimLimite &&
                    p.Fim > inicioLimite)
                .Select(p => (
                    Inicio: p.Inicio < inicioLimite
                        ? inicioLimite
                        : p.Inicio,

                    Fim: p.Fim > fimLimite
                        ? fimLimite
                        : p.Fim))
                .OrderBy(p => p.Inicio)
                .ToList();

            if (!intervalos.Any())
                return 0;

            decimal totalMinutos = 0;

            var inicioAtual = intervalos[0].Inicio;
            var fimAtual = intervalos[0].Fim;

            foreach (var intervalo in intervalos.Skip(1))
            {
                if (intervalo.Inicio <= fimAtual)
                {
                    if (intervalo.Fim > fimAtual)
                        fimAtual = intervalo.Fim;
                }
                else
                {
                    totalMinutos += (decimal)(fimAtual - inicioAtual).TotalMinutes;

                    inicioAtual = intervalo.Inicio;
                    fimAtual = intervalo.Fim;
                }
            }

            totalMinutos +=(decimal)(fimAtual - inicioAtual).TotalMinutes;

            return totalMinutos;
        }
    }
}
