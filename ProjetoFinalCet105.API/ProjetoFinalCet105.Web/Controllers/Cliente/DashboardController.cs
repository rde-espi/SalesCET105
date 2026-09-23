using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Cliente;

[Authorize(Roles = "Cliente")]
public class DashboardClienteController : Controller
{
    private readonly ApiService _apiService;

    public DashboardClienteController(ApiService apiService)
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
            var cliente = await _apiService.GetAuthenticatedAsync<ClienteViewModel>( $"api/Clientes/{userId}");

            if (cliente == null)
            {
                return NotFound();
            }

            var marcacoes = await _apiService.GetAuthenticatedAsync<List<MarcacaoClienteViewModel>>("api/Marcacoes") ?? new List<MarcacaoClienteViewModel>();

            var agora = DateTime.Now;

            var proximasMarcacoes = marcacoes
                .Where(m =>
                m.DataHoraInicio >= agora &&
                !string.Equals(
                    m.EstadoMarcacaoNome,
            "Cancelada",
            StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(
                m.EstadoMarcacaoNome,
                "Concluida",
                StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    m.EstadoMarcacaoNome,
                    "Não Compareceu",
                    StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.DataHoraInicio)
                .ToList();

            var proximaMarcacao = proximasMarcacoes.FirstOrDefault();

            var totalProximasMarcacoes = proximasMarcacoes.Count;

            var totalMarcacoesConcluidas = marcacoes.Count(m =>
                string.Equals(
                    m.EstadoMarcacaoNome,
                    "Concluida",
                    StringComparison.OrdinalIgnoreCase));

            var mensagensNaoLidas = await _apiService.GetAuthenticatedAsync<int>( "api/Conversas/contador-nao-lidas");

            var notificacoesNaoLidas = await _apiService.GetAuthenticatedAsync<int>( "api/Notificacoes/contador-nao-lidas");

            var model = new DashboardClienteViewModel
            {
                Cliente = cliente,
                ProximaMarcacao = proximaMarcacao,
                TotalProximasMarcacoes = totalProximasMarcacoes,
                TotalMarcacoesConcluidas = totalMarcacoesConcluidas,
                MensagensNaoLidas = mensagensNaoLidas,
                NotificacoesNaoLidas = notificacoesNaoLidas
            };

            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar a sua área de cliente.";

            return RedirectToAction("Index", "Home");
        }
    }
}