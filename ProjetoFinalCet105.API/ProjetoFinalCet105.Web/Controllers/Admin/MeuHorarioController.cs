using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

[Authorize(Roles = "Funcionario")]
public class MeuHorarioController : Controller
{
    private readonly ApiService _apiService;

    public MeuHorarioController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var horarios = await _apiService.GetAuthenticatedAsync<List<MeuHorarioViewModel>>("api/HorarioFuncionarios");

        horarios ??= new List<MeuHorarioViewModel>();

        var horariosOrdenados = horarios
            .Where(h => h.Ativo)
            .OrderBy(h => h.DiaSemana == DayOfWeek.Sunday ? 7 : (int)h.DiaSemana)
            .ThenBy(h => h.HoraInicio)
            .ToList();

        return View(horariosOrdenados);
    }
}