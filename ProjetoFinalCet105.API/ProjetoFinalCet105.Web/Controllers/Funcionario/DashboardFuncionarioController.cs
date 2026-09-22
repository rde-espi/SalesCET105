using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Funcionario;

[Authorize(Roles = "Funcionario")]
public class DashboardFuncionarioController : Controller
{
    private readonly ApiService _apiService;

    public DashboardFuncionarioController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            var funcionario = await _apiService.GetAuthenticatedAsync<FuncionarioViewModel>( $"api/Funcionarios/user/{userId}");

            if (funcionario == null)
            {
                return NotFound();
            }
            var marcacoes = await _apiService.GetAuthenticatedAsync<List<MarcacaoClienteViewModel>>($"api/Marcacoes/funcionario/{funcionario.Id}") ?? new List<MarcacaoClienteViewModel>();

            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);

            var inicioSemana = hoje.AddDays(-(((int)hoje.DayOfWeek + 6) % 7));
            var totalHoje = marcacoes.Count(m => m.DataHoraInicio.Date == hoje);
            var fimSemana = inicioSemana.AddDays(7);

            var totalAmanha = marcacoes.Count(m => m.DataHoraInicio.Date == amanha);

            var totalEstaSemana = marcacoes.Count(m => m.DataHoraInicio >= inicioSemana && m.DataHoraInicio < fimSemana);

            


            var marcacoesHoje = marcacoes
                .Where(m => m.DataHoraInicio.Date == hoje)
                .OrderBy(m => m.DataHoraInicio)
                .ToList();

            var mensagensNaoLidas = await _apiService.GetAuthenticatedAsync<int>("api/Conversas/contador-nao-lidas");

            var notificacoesNaoLidas = await _apiService.GetAuthenticatedAsync<int>( "api/Notificacoes/contador-nao-lidas");

            var faturas = await _apiService.GetAuthenticatedAsync<List<FaturaViewModel>>("api/Faturas") ?? new List<FaturaViewModel>();

            var marcacoesFaturadas = faturas
                .Select(f => f.MarcacaoId)
                .ToHashSet();

            var totalPorFaturar = marcacoes.Count(m => string.Equals(m.EstadoMarcacaoNome,"Concluida", StringComparison.OrdinalIgnoreCase) && !marcacoesFaturadas.Contains(m.Id));

            var model = new DashboardFuncionarioViewModel
            {
                Funcionario = funcionario,
                Data = hoje,
                MarcacoesHoje = marcacoesHoje,
                TotalPorFaturar = totalPorFaturar,
                TotalHoje = totalHoje,
                TotalAmanha = totalAmanha,
                TotalEstaSemana = totalEstaSemana,
                MensagensNaoLidas = mensagensNaoLidas,
                NotificacoesNaoLidas = notificacoesNaoLidas
            };

            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar o seu painel.";

            return RedirectToAction("Index", "Home");
        }
    }
}