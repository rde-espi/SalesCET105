using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers;

[Authorize]
public class NotificacoesController : Controller
{
    private readonly ApiService _apiService;

    public NotificacoesController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> ContadorNaoLidas()
    {
        try
        {
            var contador = await _apiService.GetAuthenticatedAsync<int>("api/Notificacoes/contador-nao-lidas");

            return Json(new
            {
                contador
            });
        }
        catch (Exception)
        {
            return Json(new
            {
                contador = 0
            });
        }
    }
}