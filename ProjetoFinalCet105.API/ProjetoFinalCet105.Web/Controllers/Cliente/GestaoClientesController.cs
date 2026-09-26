using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Cliente;

[Authorize(Roles = "Admin")]
public class GestaoClientesController : Controller
{
    private readonly ApiService _apiService;

    public GestaoClientesController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var clientes = await _apiService.GetAuthenticatedAsync<List<ClienteViewModel>>("api/Clientes");

            clientes ??= new List<ClienteViewModel>();

            return View(clientes);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os clientes.";

            return View(new List<ClienteViewModel>());
        }
    }

    public async Task<IActionResult> Detalhes(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest();

        try
        {
            var cliente = await _apiService.GetAuthenticatedAsync<ClienteViewModel>($"api/Clientes/{id}");

            if (cliente == null)
            {
                TempData["ErrorMessage"] = "Cliente não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var marcacoes = await _apiService.GetAuthenticatedAsync<List<MarcacaoClienteViewModel>>($"api/Marcacoes/cliente/{id}");

            marcacoes ??= new List<MarcacaoClienteViewModel>();

            var model = new ClienteDetalhesViewModel
            {
                Cliente = cliente,
                Marcacoes = marcacoes
                    .OrderByDescending(m => m.DataHoraInicio)
                    .ToList()
            };

            return View(model);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os detalhes do cliente.";

            return RedirectToAction(nameof(Index));
        }
    }
}