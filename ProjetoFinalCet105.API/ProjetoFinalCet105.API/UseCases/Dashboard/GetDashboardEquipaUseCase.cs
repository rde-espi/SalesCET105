using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.UseCases.Dashboard
{
    public class GetDashboardEquipaUseCase
    {
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IFaturaRepository _faturaRepository;
        private readonly IFeedbackRepository _feedbackRepository;

        public GetDashboardEquipaUseCase(
            IMarcacaoRepository marcacaoRepository,
            IFaturaRepository faturaRepository,
            IFeedbackRepository feedbackRepository)
        {
            _marcacaoRepository = marcacaoRepository;
            _faturaRepository = faturaRepository;
            _feedbackRepository = feedbackRepository;
        }

        public async Task<List<DesempenhoFuncionarioDTO>> ExecuteAsync()
        {
            var hoje = DateTime.Today;

            var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

            var inicioProximoMes = inicioMes.AddMonths(1);

            // Marcações concluídas no mês
            var marcacoes = await _marcacaoRepository
                .GetAllWithDetails()
                .Where(m =>
                    m.DataHoraInicio >= inicioMes &&
                    m.DataHoraInicio < inicioProximoMes &&
                    m.EstadoMarcacao.Nome == "Concluida")
                .Select(m => new
                {
                    m.FuncionarioId,
                    NomeFuncionario = m.Funcionario.User.NomeCompleto
                })
                .ToListAsync();

            // Faturação emitida no mês
            var faturacao = await _faturaRepository
                .GetAllWithDetails()
                .Where(f =>
                    f.DataEmissao >= inicioMes &&
                    f.DataEmissao < inicioProximoMes &&
                    f.Estado != "Anulada")
                .GroupBy(f => f.Marcacao.FuncionarioId)
                .Select(g => new
                {
                    FuncionarioId = g.Key,
                    Total = g.Sum(f => f.Total)
                })
                .ToListAsync();

            // Feedback dos funcionários
            var feedbacks = await _feedbackRepository
                .GetAllWithDetails()
                .Where(f =>
                    f.DataCriacao >= inicioMes &&
                    f.DataCriacao < inicioProximoMes)
                .GroupBy(f => f.FuncionarioId)
                .Select(g => new
                {
                    FuncionarioId = g.Key,
                    Media = g.Average(f => f.Classificacao)
                })
                .ToListAsync();

            var resultado = marcacoes
                .GroupBy(m => new
                {
                    m.FuncionarioId,
                    m.NomeFuncionario
                })
                .Select(g =>
                {
                    var totalFaturado = faturacao
                        .FirstOrDefault(f =>
                            f.FuncionarioId == g.Key.FuncionarioId);

                    var avaliacao = feedbacks
                        .FirstOrDefault(f =>
                            f.FuncionarioId == g.Key.FuncionarioId);

                    return new DesempenhoFuncionarioDTO
                    {
                        FuncionarioId = g.Key.FuncionarioId,
                        NomeFuncionario = g.Key.NomeFuncionario,

                        MarcacoesConcluidas = g.Count(),

                        TotalFaturado = totalFaturado?.Total ?? 0,

                        AvaliacaoMedia =
                            avaliacao != null
                                ? Math.Round(
                                    (decimal)avaliacao.Media,
                                    2)
                                : 0
                    };
                })
                .OrderByDescending(f => f.TotalFaturado)
                .ToList();

            return resultado;
        }
    }
}
