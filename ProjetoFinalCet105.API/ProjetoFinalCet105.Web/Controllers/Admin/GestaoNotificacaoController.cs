using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

public class GestaoNotificacoesController : Controller
{
    private readonly ApiService _apiService;

    public GestaoNotificacoesController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var notificacoes = await _apiService.GetAuthenticatedAsync<List<NotificacaoDashboardViewModel>>("api/Notificacoes")
                ?? new List<NotificacaoDashboardViewModel>();

            return View(notificacoes
                .OrderByDescending(n => n.DataCriacao)
                .ToList());
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível carregar as notificações.";

            return View(new List<NotificacaoDashboardViewModel>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarComoLida(int id)
    {
        try
        {
            var response = await _apiService.SendAuthenticatedJsonAsync<object>(HttpMethod.Put, $"api/Notificacoes/{id}/lida", null);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Não foi possível marcar a notificação como lida.";
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível marcar a notificação como lida.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarTodasComoLidas()
    {
        try
        {
            var response = await _apiService.SendAuthenticatedJsonAsync<object>(HttpMethod.Put, "api/Notificacoes/marcar-todas-lidas", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Todas as notificações foram marcadas como lidas.";
            }
            else
            {
                TempData["ErrorMessage"] = "Não foi possível atualizar as notificações.";
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível atualizar as notificações.";
        }

        return RedirectToAction(nameof(Index));
    }
}