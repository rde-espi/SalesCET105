using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Funcionario;

[Authorize(Roles = "Funcionario")]
public class MinhasCompetenciasController : Controller
{
    private readonly ApiService _apiService;

    public MinhasCompetenciasController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var competenciasFuncionario = await _apiService.GetAuthenticatedAsync<List<FuncionarioCompetenciaViewModel>>("api/FuncionarioCompetencias") ?? new List<FuncionarioCompetenciaViewModel>();

            var competenciasDisponiveis = await _apiService.GetAuthenticatedAsync<List<CompetenciaViewModel>>("api/Competencias") ?? new List<CompetenciaViewModel>();

            var model = new MinhasCompetenciasViewModel
            {
                CompetenciasFuncionario = competenciasFuncionario
                    .OrderBy(c => c.CompetenciaNome)
                    .ToList(),

                CompetenciasDisponiveis = competenciasDisponiveis
                    .Where(c => c.Ativa)
                    .OrderBy(c => c.Nome)
                    .ToList()
            };

            return View(model);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível carregar as suas competências.";

            return View(new MinhasCompetenciasViewModel());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adicionar(FuncionarioCompetenciaRequestViewModel model)
    {
        if (model.CompetenciaId <= 0)
        {
            TempData["ErrorMessage"] = "Selecione uma competência válida.";

            return RedirectToAction(nameof(Index));
        }

        using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Post, "api/FuncionarioCompetencias", model);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(erro) ? "Não foi possível adicionar a competência." : erro;

            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Competência adicionada com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remover(int id)
    {
        if (id <= 0)
        {
            TempData["ErrorMessage"] = "Competência inválida.";

            return RedirectToAction(nameof(Index));
        }

        using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Delete, $"api/FuncionarioCompetencias/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(erro) ? "Não foi possível remover a competência." : erro;

            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Competência removida com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Alterar(FuncionarioCompetenciaRequestViewModel model)
    {
        if (model.Id <= 0 || model.CompetenciaId <= 0)
        {
            TempData["ErrorMessage"] = "Competência inválida.";

            return RedirectToAction(nameof(Index));
        }

        using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Put, $"api/FuncionarioCompetencias/{model.Id}", model);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(erro) ? "Não foi possível atualizar a competência." : erro;

            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Competência atualizada com sucesso.";

        return RedirectToAction(nameof(Index));
    }
}