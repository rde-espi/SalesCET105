using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Funcionario;

[Authorize(Roles = "Funcionario")]
public class FeedbackFuncionarioController : Controller
{
    private readonly ApiService _apiService;

    public FeedbackFuncionarioController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var feedbacks = await _apiService.GetAuthenticatedAsync<List<FeedbackViewModel>>( "api/Feedbacks/meus") ?? new List<FeedbackViewModel>();

            var resumo = await _apiService.GetAuthenticatedAsync<FeedbackResumoViewModel>( "api/Feedbacks/meus/resumo") ?? new FeedbackResumoViewModel();

            var model = new FeedbackFuncionarioViewModel
            {
                Resumo = resumo,

                Feedbacks = feedbacks
                    .OrderByDescending(f => f.DataCriacao)
                    .ToList()
            };

            return View(model);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os seus feedbacks.";

            return View(new FeedbackFuncionarioViewModel());
        }
    }
}
