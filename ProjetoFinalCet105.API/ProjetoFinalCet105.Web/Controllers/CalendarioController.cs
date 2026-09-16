using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers;

[Authorize]
public class CalendarioController : Controller
{
    private readonly ApiService _apiService;

    public CalendarioController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index(DateTime? data, int? funcionarioId, string vista = "semana")
    {
        try
        {
            var dataReferencia = (data ?? DateTime.Today).Date;

            var marcacoes = await _apiService.GetAuthenticatedAsync<List<MarcacaoClienteViewModel>>("api/Marcacoes")
                ?? new List<MarcacaoClienteViewModel>();

            var funcionarios = await _apiService.GetAuthenticatedAsync<List<FuncionarioViewModel>>("api/Funcionarios")
                ?? new List<FuncionarioViewModel>();

            vista = vista?.ToLowerInvariant() ?? "semana";

            if (vista != "dia" &&
                vista != "semana" &&
                vista != "mes")
            {
                vista = "semana";
            }

            ViewBag.DataReferencia = dataReferencia;
            ViewBag.FuncionarioId = funcionarioId;
            ViewBag.Vista = vista;

            ViewBag.Funcionarios = funcionarios
                .Where(f => f.Ativo)
                .OrderBy(f => f.NomeCompleto)
                .ToList();

            if (funcionarioId.HasValue)
            {
                marcacoes = marcacoes
                    .Where(m => m.FuncionarioId == funcionarioId.Value)
                    .ToList();
            }

            return View(marcacoes);
        }
        catch (Exception)
        {
            ViewBag.DataReferencia = (data ?? DateTime.Today).Date;
            ViewBag.FuncionarioId = funcionarioId;
            ViewBag.Funcionarios = new List<FuncionarioViewModel>();

            return View(new List<MarcacaoClienteViewModel>());
        }
    }
}