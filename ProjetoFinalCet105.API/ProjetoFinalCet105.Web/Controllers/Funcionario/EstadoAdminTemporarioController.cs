using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Funcionario;

[Authorize]
public class EstadoAdminTemporarioController : Controller
{
    private readonly ApiService _apiService;

    public EstadoAdminTemporarioController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> MeuEstado()
    {
        try
        {
            var estado = await _apiService.GetAuthenticatedAsync<EstadoAdminTemporarioViewModel>( "api/EstadoAdminTemporario/me");

            if (estado == null)
            {
                return Json(new
                {
                    ativo = false,
                    dataInicio = (DateTime?)null,
                    dataFim = (DateTime?)null
                });
            }

            return Json(new
            {
                ativo = estado.Ativo,
                dataInicio = estado.DataInicio,
                dataFim = estado.DataFim
            });
        }
        catch
        {
            return Json(new
            {
                ativo = false,
                dataInicio = (DateTime?)null,
                dataFim = (DateTime?)null
            });
        }
    }
}
