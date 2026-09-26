using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Funcionario;

[Authorize(Roles = "Funcionario")]
public class MarcacoesFuncionarioController : Controller
{
    private readonly ApiService _apiService;

    public MarcacoesFuncionarioController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var marcacao = await _apiService.GetAuthenticatedAsync<MarcacaoClienteViewModel>($"api/Marcacoes/{id}");

            if (marcacao == null)
            {
                return NotFound();
            }

            return View(marcacao);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os detalhes da marcação.";

            return RedirectToAction("Index", "MinhaAgenda");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(int id)
    {
        return await AlterarEstado(id, 2, "Marcação confirmada com sucesso.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Concluir(int id)
    {
        return await AlterarEstado(id, 3, "Marcação concluída com sucesso.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NaoCompareceu(int id)
    {
        return await AlterarEstado(id, 5, "Marcação registada como não compareceu.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        if (id <= 0)
            return BadRequest();

        try
        {
            using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Delete, $"api/Marcacoes/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Não foi possível cancelar a marcação.";

                return RedirectToAction(nameof(Detalhes), new { id });
            }

            TempData["SuccessMessage"] = "Marcação cancelada com sucesso.";

            return RedirectToAction("Index", "MinhaAgenda");
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível cancelar a marcação.";

            return RedirectToAction(nameof(Detalhes), new { id });
        }
    }

    private async Task<IActionResult> AlterarEstado(int id, int estadoMarcacaoId, string mensagemSucesso)
    {
        if (id <= 0)
            return BadRequest();

        try
        {
            var dto = new
            {
                EstadoMarcacaoId = estadoMarcacaoId
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Patch, $"api/Marcacoes/{id}/estado", dto);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Não foi possível alterar o estado da marcação.";

                return RedirectToAction(nameof(Detalhes), new { id });
            }

            TempData["SuccessMessage"] = mensagemSucesso;

            return RedirectToAction(nameof(Detalhes), new { id });
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível alterar o estado da marcação.";

            return RedirectToAction(nameof(Detalhes), new { id });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var marcacao = await _apiService.GetAuthenticatedAsync<MarcacaoClienteViewModel>($"api/Marcacoes/{id}");

            if (marcacao == null)
            {
                TempData["ErrorMessage"] = "A marcação não foi encontrada.";
                return RedirectToAction("Index", "MinhaAgenda");
            }

            var servicos = await _apiService.GetAuthenticatedAsync<List<FuncionarioServicoViewModel>>($"api/FuncionarioServicos/funcionario/{marcacao.FuncionarioId}");

            var model = new EditarMarcacaoViewModel
            {
                Id = marcacao.Id,
                ServicoId = marcacao.ServicoId,
                FuncionarioId = marcacao.FuncionarioId,
                DataHoraInicio = marcacao.DataHoraInicio,
                Observacoes = marcacao.Observacoes,
                ClienteNome = marcacao.ClienteNome,
                FuncionarioNome = marcacao.FuncionarioNome,

                Servicos = servicos?
                    .Where(s => s.Ativo)
                    .OrderBy(s => s.ServicoNome)
                    .ToList()
                    ?? new List<FuncionarioServicoViewModel>()
            };

            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar a marcação.";

            return RedirectToAction("Index", "MinhaAgenda");
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarMarcacaoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CarregarServicosEdicao(model);
            return View(model);
        }

        try
        {
            var dto = new
            {
                model.ServicoId,
                model.DataHoraInicio,
                model.Observacoes
            };

            using var response =
                await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Put, $"api/Marcacoes/{model.Id}", dto);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(erro)
                        ? "Não foi possível alterar a marcação."
                        : erro.Trim('"'));

                await CarregarServicosEdicao(model);

                return View(model);
            }

            TempData["SuccessMessage"] = "Marcação alterada com sucesso.";

            return RedirectToAction(nameof(Detalhes), new { id = model.Id });
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao alterar a marcação.");

            await CarregarServicosEdicao(model);

            return View(model);
        }
    }


    private async Task CarregarServicosEdicao(EditarMarcacaoViewModel model)
    {
        model.Servicos = await _apiService.GetAuthenticatedAsync<List<FuncionarioServicoViewModel>>($"api/FuncionarioServicos/funcionario/{model.FuncionarioId}")
            ?? new List<FuncionarioServicoViewModel>();

        model.Servicos = model.Servicos
            .Where(s => s.Ativo)
            .OrderBy(s => s.ServicoNome)
            .ToList();
    }


    [HttpGet]
    public async Task<IActionResult> HorariosDisponiveis(int funcionarioId, int servicoId, DateTime data)
    {
        try
        {
            var horarios = await _apiService.GetAuthenticatedAsync<List<DateTime>>(
                $"api/Marcacoes/disponibilidade" +
                $"?funcionarioId={funcionarioId}" +
                $"&servicoId={servicoId}" +
                $"&data={data:yyyy-MM-dd}");

            return Json(horarios ?? new List<DateTime>());
        }
        catch
        {
            return Json(new List<DateTime>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Cliente(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        try
        {
            var marcacoes = await _apiService.GetAuthenticatedAsync<List<MarcacaoClienteViewModel>>("api/Marcacoes") ?? new List<MarcacaoClienteViewModel>();

            var marcacoesCliente = marcacoes
                .Where(m => m.ClienteId == id)
                .ToList();

            if (!marcacoesCliente.Any())
            {
                return NotFound();
            }

            var cliente = marcacoesCliente
                .OrderByDescending(m => m.DataHoraInicio)
                .First();

            var estadosFinais = new[]
            {
                "Concluida",
                "Cancelada",
                "Não Compareceu"
            };

            var agora = DateTime.Now;

            var historico = marcacoesCliente
                .Where(m =>
                    m.DataHoraFim < agora &&
                    !string.IsNullOrWhiteSpace(m.EstadoMarcacaoNome) &&
                    estadosFinais.Contains(
                        m.EstadoMarcacaoNome,
                        StringComparer.OrdinalIgnoreCase))
                .OrderByDescending(m => m.DataHoraInicio)
                .ToList();

            var model = new ClienteHistoricoFuncionarioViewModel
            {
                ClienteId = cliente.ClienteId,
                ClienteNome = cliente.ClienteNome,
                Historico = historico
            };

            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os detalhes do cliente.";

            return RedirectToAction("Index", "MinhaAgenda");
        }
    }
}