using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using System.Globalization;

namespace ProjetoFinalCet105.API.UseCases.Dashboard
{
    public class GetDashboardFinanceiroUseCase
    {
        private readonly IFaturaRepository _faturaRepository;
        private readonly IDespesaRepository _despesaRepository;

        public GetDashboardFinanceiroUseCase(IFaturaRepository faturaRepository, IDespesaRepository despesaRepository)
        {
            _faturaRepository = faturaRepository;
            _despesaRepository = despesaRepository;
        }

        public async Task<DashboardFinanceiroDTO> ExecuteAsync()
        {
            var hoje = DateTime.Today;

            var inicioSemana = hoje.AddDays(-(((int)hoje.DayOfWeek + 6) % 7));

            var inicioMes = new DateTime( hoje.Year, hoje.Month, 1);

            var amanha = hoje.AddDays(1);
            var inicioProximoMes = inicioMes.AddMonths(1);

            // Apenas documentos válidos entram nos indicadores financeiros.
            var faturas = _faturaRepository
                .GetAllWithDetails()
                .Where(f => f.Estado != "Anulada");

            var faturacaoHoje = await faturas
                .Where(f =>
                    f.DataEmissao >= hoje &&
                    f.DataEmissao < amanha)
                .SumAsync(f => (decimal?)f.Total) ?? 0;

            var faturacaoSemana = await faturas
                .Where(f =>
                    f.DataEmissao >= inicioSemana &&
                    f.DataEmissao < amanha)
                .SumAsync(f => (decimal?)f.Total) ?? 0;

            var faturasMes = faturas
                .Where(f =>
                    f.DataEmissao >= inicioMes &&
                    f.DataEmissao < inicioProximoMes);

            var faturacaoMes = await faturasMes.SumAsync(f => (decimal?)f.Total) ?? 0;

            var totalFaturasMes = await faturasMes.CountAsync();

            var ticketMedioMes =
                totalFaturasMes > 0
                    ? faturacaoMes / totalFaturasMes
                    : 0;

            var faturasComNifMes = await faturasMes
                .CountAsync(f =>
                    f.NifCliente != null &&
                    f.NifCliente != "");

            var faturasSemNifMes = totalFaturasMes - faturasComNifMes;

            var valorComNifMes = await faturasMes
                .Where(f =>
                    f.NifCliente != null &&
                    f.NifCliente != "")
                .SumAsync(f => (decimal?)f.Total) ?? 0;

            var valorSemNifMes = faturacaoMes - valorComNifMes;

            var despesasMes = await _despesaRepository
                .GetAllDespesas()
                .Where(d =>
                d.DataDespesa >= inicioMes &&
                d.DataDespesa < inicioProximoMes)
                .SumAsync(d => (decimal?)d.Valor) ?? 0;

            var resultadoMes = faturacaoMes - despesasMes;

            var margemPercentualMes =
                faturacaoMes > 0
                    ? (resultadoMes / faturacaoMes) * 100
                    : 0;

            return new DashboardFinanceiroDTO
            {
                FaturacaoHoje = faturacaoHoje,
                FaturacaoSemana = faturacaoSemana,
                FaturacaoMes = faturacaoMes,

                TotalFaturasMes = totalFaturasMes,

                TicketMedioMes = Math.Round(ticketMedioMes, 2),

                FaturasComNifMes = faturasComNifMes,
                FaturasSemNifMes = faturasSemNifMes,

                ValorComNifMes = valorComNifMes,
                ValorSemNifMes = valorSemNifMes,

                DespesasMes = despesasMes,
                ResultadoMes = resultadoMes,
                MargemPercentualMes = Math.Round(margemPercentualMes, 2)
            };
        }
        public async Task<List<FaturacaoMensalDTO>> ExecuteEvolucaoMensalAsync( int? ano = null)
        {
            var anoSelecionado = ano ?? DateTime.Today.Year;

            var dados = await _faturaRepository
                .GetAllWithDetails()
                .Where(f =>
                    f.Estado != "Anulada" &&
                    f.DataEmissao.Year == anoSelecionado)
                .GroupBy(f => new
                {
                    Ano = f.DataEmissao.Year,
                    Mes = f.DataEmissao.Month
                })
                .Select(g => new
                {
                    g.Key.Ano,
                    g.Key.Mes,
                    Total = g.Sum(f => f.Total)
                })
                .OrderBy(x => x.Mes)
                .ToListAsync();

            var cultura = new CultureInfo("pt-PT");

            return dados
                .Select(x => new FaturacaoMensalDTO
                {
                    Ano = x.Ano,
                    Mes = x.Mes,

                    NomeMes = cultura.TextInfo.ToTitleCase( cultura.DateTimeFormat.GetMonthName(x.Mes)),

                    Total = x.Total
                })
                .ToList();
        }

        public async Task<List<ServicoFaturacaoDTO>> ExecuteServicosMaisFaturadosAsync(int limite = 5)
        {
            var dados = await _faturaRepository
                .GetAllWithDetails()
                .Where(f => f.Estado != "Anulada")
                .SelectMany(f => f.Itens)
                .GroupBy(item => new
                {
                    item.ServicoId,
                    item.Descricao
                })
                .Select(g => new ServicoFaturacaoDTO
                {
                    ServicoId = g.Key.ServicoId,
                    NomeServico = g.Key.Descricao,
                    Quantidade = (int)g.Sum(x => x.Quantidade),
                    TotalFaturado = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.TotalFaturado)
                .Take(limite)
                .ToListAsync();

            return dados;
        }

        public async Task<List<CategoriaFaturacaoDTO>>ExecuteFaturacaoPorCategoriaAsync()
        {
            var dados = await _faturaRepository
                .GetAllWithDetails()
                .Where(f =>
                    f.Estado != "Anulada" &&
                    f.Marcacao.Servico.Categoria != null)
                .GroupBy(f => new
                {
                    CategoriaId = f.Marcacao.Servico.CategoriaId,
                    NomeCategoria = f.Marcacao.Servico.Categoria!.Nome
                })
                .Select(g => new CategoriaFaturacaoDTO
                {
                    CategoriaId = g.Key.CategoriaId,
                    NomeCategoria = g.Key.NomeCategoria,

                    QuantidadeServicos = (int)g
                        .SelectMany(f => f.Itens)
                        .Sum(i => i.Quantidade),

                    TotalFaturado = g.Sum(f => f.Total)
                })
                .OrderByDescending(x => x.TotalFaturado)
                .ToListAsync();

            return dados;
        }
    }
}
