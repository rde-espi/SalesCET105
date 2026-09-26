using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Models;

public class MensagensNaoLidasViewComponent : ViewComponent
{
    private readonly ApiService _apiService;

    public MensagensNaoLidasViewComponent(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var contador = await _apiService.GetAuthenticatedAsync<int>("api/Conversas/contador-nao-lidas");

        return View(contador);
    }
}
