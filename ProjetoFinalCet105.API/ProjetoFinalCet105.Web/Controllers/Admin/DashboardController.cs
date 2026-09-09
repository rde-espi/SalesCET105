using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApiService _apiService;

        public DashboardController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var financeiro = await _apiService.GetAuthenticatedAsync<DashboardFinanceiroViewModel>("api/Dashboard/financeiro");

            var agenda = await _apiService.GetAuthenticatedAsync<DashboardAgendaViewModel>( "api/Dashboard/agenda");

            var clientes = await _apiService.GetAuthenticatedAsync<DashboardClientesViewModel>( "api/Dashboard/clientes");
            var servicosMaisFaturados = await _apiService.GetAuthenticatedAsync<List<ServicoFaturacaoViewModel>>("api/Dashboard/financeiro/servicos?limite=5");
            var faturacaoPorCategoria = await _apiService.GetAuthenticatedAsync<List<FaturacaoCategoriaViewModel>>("api/Dashboard/financeiro/categorias");
            var todasMarcacoes = await _apiService.GetAuthenticatedAsync<List<MarcacaoDashboardViewModel>>("api/Marcacoes");
            var todosClientes = await _apiService.GetAuthenticatedAsync<List<ClienteRecenteViewModel>>("api/Clientes");
            var todasNotificacoes = await _apiService.GetAuthenticatedAsync<List<NotificacaoDashboardViewModel>>("api/Notificacoes");
            var equipa = await _apiService.GetAuthenticatedAsync<List<DesempenhoFuncionarioViewModel>>("api/Dashboard/equipa");

            var anoAtual = DateTime.Today.Year;
            var anoAnterior = anoAtual - 1;

            var evolucaoAnoAtual =
                await _apiService.GetAuthenticatedAsync<List<FaturacaoMensalViewModel>>(
                    $"api/Dashboard/financeiro/evolucao-mensal?ano={anoAtual}");

            var evolucaoAnoAnterior =
                await _apiService.GetAuthenticatedAsync<List<FaturacaoMensalViewModel>>(
                    $"api/Dashboard/financeiro/evolucao-mensal?ano={anoAnterior}");

            var evolucaoMensalCompleta =
                (evolucaoAnoAnterior ?? new List<FaturacaoMensalViewModel>())
                    .Concat(evolucaoAnoAtual ?? new List<FaturacaoMensalViewModel>())
                    .OrderBy(x => x.Ano)
                    .ThenBy(x => x.Mes)
                    .ToList();

            var evolucaoMensal =
                evolucaoAnoAtual ?? new List<FaturacaoMensalViewModel>();

            var notificacoesRecentes =
                todasNotificacoes?
                .Take(5)
                .ToList()
                ?? new List<NotificacaoDashboardViewModel>();

            var notificacoesNaoLidas = await _apiService.GetAuthenticatedAsync<int>("api/Notificacoes/contador-nao-lidas");

            var ultimosClientes =
                todosClientes?
                .OrderByDescending(c => c.DataCriacao)
                .Take(5)
                .ToList()
                ?? new List<ClienteRecenteViewModel>();

            var hoje = DateTime.Today;

            var marcacoesHoje =
                todasMarcacoes?
                    .Where(m => m.DataHoraInicio.Date == hoje)
                    .OrderBy(m => m.DataHoraInicio)
                    .Take(5)
                    .ToList()
                ?? new List<MarcacaoDashboardViewModel>();

            var model = new DashboardViewModel
            {
                Financeiro = financeiro,
                Agenda = agenda,
                Clientes = clientes,
                EvolucaoMensal = evolucaoMensal ?? new List<FaturacaoMensalViewModel>(),
                FaturacaoPorCategoria = faturacaoPorCategoria ?? new List<FaturacaoCategoriaViewModel>(),
                ServicosMaisFaturados = servicosMaisFaturados ?? new List<ServicoFaturacaoViewModel>(),
                MarcacoesHoje = marcacoesHoje,
                UltimosClientes = ultimosClientes,
                NotificacoesRecentes = notificacoesRecentes,
                NotificacoesNaoLidas = notificacoesNaoLidas,
                EvolucaoMensalCompleta = evolucaoMensalCompleta,
                Equipa = equipa ?? new List<DesempenhoFuncionarioViewModel>()
            };
                     
            return View(model);
        }

    }
}
