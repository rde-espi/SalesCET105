using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class GestaoMarcacoesController : Controller
{
    private readonly ApiService _apiService;

    public GestaoMarcacoesController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var marcacoes = await _apiService.GetAuthenticatedAsync<List<MarcacaoClienteViewModel>>( "api/Marcacoes");

            marcacoes ??= new List<MarcacaoClienteViewModel>();

            return View(
                marcacoes
                    .OrderByDescending(m => m.DataHoraInicio)
                    .ToList());
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível carregar as marcações.";

            return View(new List<MarcacaoClienteViewModel>());
        }
    }
}
