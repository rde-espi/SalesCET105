using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Funcionario;

[Authorize(Roles = "Funcionario")]
public class NovaMarcacaoFuncionarioController : Controller
{
    private readonly ApiService _apiService;

    public NovaMarcacaoFuncionarioController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
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

            var model = new NovaMarcacaoFuncionarioViewModel();

            await CarregarDados(model, funcionario.Id);

            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível preparar a nova marcação.";

            return RedirectToAction("Index", "MinhaAgenda");
        }
    }

    [HttpGet]
    public async Task<IActionResult> HorariosDisponiveis(int servicoId, DateTime data)
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

            var horarios = await _apiService.GetAuthenticatedAsync<List<DateTime>>(
                $"api/Marcacoes/disponibilidade" +
                $"?funcionarioId={funcionario.Id}" +
                $"&servicoId={servicoId}" +
                $"&data={data:yyyy-MM-dd}");

            return Json(horarios ?? new List<DateTime>());
        }
        catch
        {
            return Json(new List<DateTime>());
        }
    }

    private async Task CarregarDados(NovaMarcacaoFuncionarioViewModel model, int funcionarioId)
    {
        var clientes = await _apiService.GetAuthenticatedAsync<List<ClienteViewModel>>("api/Clientes");

        var funcionarioServicos = await _apiService.GetAuthenticatedAsync<List<FuncionarioServicoViewModel>>($"api/FuncionarioServicos/funcionario/{funcionarioId}");

        var servicos = await _apiService.GetAuthenticatedAsync<List<ServicoViewModel>>("api/Servicos");

        model.Clientes = clientes?
            .Where(c => c.Ativo)
            .OrderBy(c => c.NomeCompleto)
            .ToList()
            ?? new List<ClienteViewModel>();

        var servicosAtivosFuncionario = funcionarioServicos?
            .Where(fs => fs.Ativo)
            .Select(fs => fs.ServicoId)
            .ToHashSet()
            ?? new HashSet<int>();

        model.Servicos = servicos?
            .Where(s =>
                s.Disponivel &&
                servicosAtivosFuncionario.Contains(s.Id))
            .OrderBy(s => s.Nome)
            .ToList()
            ?? new List<ServicoViewModel>();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(NovaMarcacaoFuncionarioViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        FuncionarioViewModel? funcionario;

        try
        {
            funcionario = await _apiService.GetAuthenticatedAsync<FuncionarioViewModel>($"api/Funcionarios/user/{userId}");

            if (funcionario == null)
            {
                return NotFound();
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível identificar o funcionário.";

            return RedirectToAction("Index", "MinhaAgenda");
        }

        if (!ModelState.IsValid)
        {
            await CarregarDados(model, funcionario.Id);

            return View(model);
        }

        try
        {
            var dto = new
            {
                model.ClienteId,
                model.ServicoId,
                model.DataHoraInicio,
                model.Observacoes
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Post, "api/Marcacoes", dto);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(string.Empty, string.IsNullOrWhiteSpace(erro) ? "Não foi possível criar a marcação." : erro.Trim('"'));

                await CarregarDados(model, funcionario.Id);

                return View(model);
            }

            TempData["SuccessMessage"] = "Marcação criada com sucesso.";

            return RedirectToAction("Index", "MinhaAgenda");
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao criar a marcação.");

            await CarregarDados(model, funcionario.Id);

            return View(model);
        }
    }
}