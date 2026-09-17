using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers;

public class GestaoFeedbacksController : Controller
{
    private readonly ApiService _apiService;

    public GestaoFeedbacksController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var feedbacks = await _apiService.GetAuthenticatedAsync<List<FeedbackViewModel>>("api/Feedbacks") ?? new List<FeedbackViewModel>();

            feedbacks = feedbacks
                .OrderByDescending(f => f.DataCriacao)
                .ToList();

            var model = new GestaoFeedbacksViewModel
            {
                Feedbacks = feedbacks,

                TotalFeedbacks = feedbacks.Count,

                MediaGeral = feedbacks.Any()
                    ? feedbacks.Average(f => f.Classificacao)
                    : 0,

                CincoEstrelas = feedbacks.Count(
                    f => f.Classificacao == 5),

                ProfissionaisAvaliados = feedbacks
                    .Select(f => f.FuncionarioId)
                    .Distinct()
                    .Count()
            };

            return View(model);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os feedbacks.";

            return View(new GestaoFeedbacksViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        try
        {
            var feedback = await _apiService.GetAuthenticatedAsync<FeedbackViewModel>($"api/Feedbacks/{id}");

            if (feedback == null)
                return NotFound();

            return View(feedback);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var feedback =
                await _apiService.GetAuthenticatedAsync<FeedbackViewModel>(
                    $"api/Feedbacks/{id}");

            if (feedback == null)
                return NotFound();

            return View(feedback);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, FeedbackViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        try
        {
            var request = new
            {
                Classificacao = model.Classificacao,
                Comentario = model.Comentario
            };

            var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Put, $"api/Feedbacks/{id}", request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Feedback atualizado com sucesso.";

                return RedirectToAction(nameof(Detalhes), new { id });
            }

            TempData["ErrorMessage"] = "Não foi possível atualizar o feedback.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível atualizar o feedback.";
        }

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var response = await _apiService.SendAuthenticatedJsonAsync<object>(HttpMethod.Delete, $"api/Feedbacks/{id}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Feedback eliminado com sucesso.";

                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Não foi possível eliminar o feedback.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível eliminar o feedback.";
        }

        return RedirectToAction(nameof(Detalhes), new { id });
    }
}