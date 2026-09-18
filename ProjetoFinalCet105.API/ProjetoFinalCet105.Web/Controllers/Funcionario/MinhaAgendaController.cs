using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Funcionario;

[Authorize(Roles = "Funcionario")]
public class MinhaAgendaController : Controller
{
    private readonly ApiService _apiService;

    public MinhaAgendaController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? data)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            var funcionario = await _apiService.GetAuthenticatedAsync<FuncionarioViewModel>($"api/Funcionarios/user/{userId}");

            if (funcionario == null)
            {
                return NotFound();
            }

            var dataSelecionada = (data ?? DateTime.Today).Date;

            var marcacoes = await _apiService.GetAuthenticatedAsync<List<MarcacaoClienteViewModel>>($"api/Marcacoes/funcionario/{funcionario.Id}") ?? new List<MarcacaoClienteViewModel>();

            var inicioMes = new DateTime(dataSelecionada.Year, dataSelecionada.Month, 1);

            var fimMes = inicioMes.AddMonths(1);

            var marcacoesMes = marcacoes
                .Where(m =>
                    m.DataHoraInicio >= inicioMes &&
                    m.DataHoraInicio < fimMes)
                .OrderBy(m => m.DataHoraInicio)
                .ToList();

            var marcacoesDia = marcacoesMes
                .Where(m =>
                    m.DataHoraInicio.Date == dataSelecionada)
                .OrderBy(m => m.DataHoraInicio)
                .ToList();

            var model = new MinhaAgendaViewModel
            {
                FuncionarioId = funcionario.Id,
                FuncionarioNome = funcionario.NomeCompleto,

                MesSelecionado = inicioMes,
                DataSelecionada = dataSelecionada,

                MarcacoesMes = marcacoesMes,
                MarcacoesDia = marcacoesDia
            };

            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar a sua agenda.";

            return RedirectToAction("Index", "Home");
        }
    }
}
