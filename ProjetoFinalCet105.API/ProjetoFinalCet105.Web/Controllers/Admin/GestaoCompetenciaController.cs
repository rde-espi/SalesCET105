using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class GestaoCompetenciasController : Controller
{
    private readonly ApiService _apiService;

    public GestaoCompetenciasController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var competencias = await _apiService.GetAuthenticatedAsync<List<CompetenciaViewModel>>( "api/Competencias");

        competencias ??= new List<CompetenciaViewModel>();

        return View(
            competencias
                .OrderByDescending(c => c.Ativa)
                .ThenBy(c => c.Nome)
                .ToList());
    }

    [HttpGet]
    public IActionResult Criar()
    {
        var model = new CompetenciaFormViewModel
        {
            Ativa = true
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CompetenciaFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var competencia = new CompetenciaViewModel
        {
            Nome = model.Nome,
            Descricao = model.Descricao,
            Ativa = true
        };

        using var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Post,"api/Competencias", competencia);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(erro)
                    ? "Não foi possível criar a competência."
                    : erro);

            return View(model);
        }

        TempData["SuccessMessage"] = "Competência criada com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var competencia = await _apiService.GetAuthenticatedAsync<CompetenciaViewModel>( $"api/Competencias/{id}");

        if (competencia == null)
        {
            return NotFound();
        }

        var model = new CompetenciaFormViewModel
        {
            Id = competencia.Id,
            Nome = competencia.Nome,
            Descricao = competencia.Descricao,
            Ativa = competencia.Ativa
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, CompetenciaFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var competencia = new CompetenciaViewModel
        {
            Id = model.Id,
            Nome = model.Nome,
            Descricao = model.Descricao,
            Ativa = model.Ativa
        };

        using var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Put, $"api/Competencias/{model.Id}", competencia);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(erro)
                    ? "Não foi possível atualizar a competência."
                    : erro);

            return View(model);
        }

        TempData["SuccessMessage"] = "Competência atualizada com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desativar(int id)
    {
        using var response = await _apiService.SendAuthenticatedAsync( HttpMethod.Delete, $"api/Competencias/{id}");

        if (!response.IsSuccessStatusCode)
        {
            TempData["ErrorMessage"] = "Não foi possível desativar a competência.";

            return RedirectToAction(nameof(Editar), new { id });
        }

        TempData["SuccessMessage"] = "Competência desativada com sucesso.";

        return RedirectToAction(nameof(Index));
    }
}
