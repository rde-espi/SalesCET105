using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

public class GestaoMensagensController : Controller
{
    private readonly ApiService _apiService;

    public GestaoMensagensController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var conversas = await _apiService.GetAuthenticatedAsync<List<ConversaViewModel>>("api/Conversas") ?? new List<ConversaViewModel>();

            var model = new GestaoMensagensViewModel
            {
                Conversas = conversas,
                TotalConversas = conversas.Count,
                TotalClientes = conversas
                    .Select(c => c.ClienteId)
                    .Distinct()
                    .Count(),
                TotalProfissionais = conversas
                    .Select(c => c.FuncionarioUserId)
                    .Distinct()
                    .Count()
            };

            return View(model);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível carregar as conversas.";

            return View(new GestaoMensagensViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        try
        {
            var conversa = await _apiService.GetAuthenticatedAsync<ConversaViewModel>($"api/Conversas/{id}");

            if (conversa == null)
                return NotFound();

            return View(conversa);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }
}