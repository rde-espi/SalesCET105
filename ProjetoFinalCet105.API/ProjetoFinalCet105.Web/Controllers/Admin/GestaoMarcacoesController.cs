using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class GestaoMarcacoesController : Controller
{
    private readonly ApiService _apiService;

    public GestaoMarcacoesController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var marcacoes = await _apiService.GetAuthenticatedAsync<List<MarcacaoClienteViewModel>>( "api/Marcacoes");

            marcacoes ??= new List<MarcacaoClienteViewModel>();

            return View(
                marcacoes
                    .OrderByDescending(m => m.DataHoraInicio)
                    .ToList());
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível carregar as marcações.";

            return View(new List<MarcacaoClienteViewModel>());
        }
    }

    public async Task<IActionResult> Detalhes(int id)
    {
        try
        {
            var marcacao = await _apiService.GetAuthenticatedAsync<MarcacaoClienteViewModel>( $"api/Marcacoes/{id}");

            if (marcacao == null)
            {
                TempData["ErrorMessage"] = "A marcação não foi encontrada.";

                return RedirectToAction(nameof(Index));
            }

            var historico = await _apiService.GetAuthenticatedAsync<List<HistoricoMarcacaoViewModel>>( $"api/Marcacoes/{id}/historico");

            var model = new MarcacaoDetalhesViewModel
            {
                Marcacao = marcacao,
                Historico = historico?
                    .OrderByDescending(h => h.DataAlteracao)
                    .ToList()
                    ?? new List<HistoricoMarcacaoViewModel>()
            };

            return View(model);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os detalhes da marcação.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(int id)
    {
        try
        {
            var dto = new UpdateEstadoMarcacaoViewModel
            {
                EstadoMarcacaoId = 2
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Patch, $"api/Marcacoes/{id}/estado",dto);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Não foi possível confirmar a marcação.";

                return RedirectToAction( nameof(Detalhes), new { id });
            }

            TempData["SuccessMessage"] = "Marcação confirmada com sucesso.";

            return RedirectToAction(nameof(Detalhes),  new { id });
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Ocorreu um erro ao confirmar a marcação.";

            return RedirectToAction( nameof(Detalhes), new { id });
        }
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

    private async Task<IActionResult> AlterarEstado( int id, int estadoMarcacaoId, string mensagemSucesso)
    {
        try
        {
            var dto = new UpdateEstadoMarcacaoViewModel
            {
                EstadoMarcacaoId = estadoMarcacaoId
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Patch, $"api/Marcacoes/{id}/estado", dto);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Não foi possível alterar o estado da marcação.";

                return RedirectToAction( nameof(Detalhes), new { id });
            }

            TempData["SuccessMessage"] = mensagemSucesso;

            return RedirectToAction( nameof(Detalhes), new { id });
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Ocorreu um erro ao alterar o estado da marcação.";

            return RedirectToAction( nameof(Detalhes),  new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        try
        {
            using var response = await _apiService.SendAuthenticatedAsync( HttpMethod.Delete, $"api/Marcacoes/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Não foi possível cancelar a marcação.";

                return RedirectToAction( nameof(Detalhes), new { id });
            }

            TempData["SuccessMessage"] = "Marcação cancelada com sucesso.";

            return RedirectToAction( nameof(Detalhes), new { id });
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Ocorreu um erro ao cancelar a marcação.";

            return RedirectToAction(nameof(Detalhes), new { id });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var marcacao =  await _apiService.GetAuthenticatedAsync<MarcacaoClienteViewModel>( $"api/Marcacoes/{id}");

            if (marcacao == null)
            {
                TempData["ErrorMessage"] = "A marcação não foi encontrada.";
                return RedirectToAction(nameof(Index));
            }

            var servicos = await _apiService.GetAuthenticatedAsync<List<FuncionarioServicoViewModel>>( $"api/FuncionarioServicos/funcionario/{marcacao.FuncionarioId}");
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
        catch (Exception)
        {
            TempData["ErrorMessage"] =  "Não foi possível carregar a marcação.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarMarcacaoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Servicos = await _apiService.GetAuthenticatedAsync<List<FuncionarioServicoViewModel>>( $"api/FuncionarioServicos/funcionario/{model.FuncionarioId}")
                ?? new List<FuncionarioServicoViewModel>();

            model.Servicos = model.Servicos
                .Where(s => s.Ativo)
                .OrderBy(s => s.ServicoNome)
                .ToList();

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

            using var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Put, $"api/Marcacoes/{model.Id}", dto);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(erro)
                        ? "Não foi possível alterar a marcação."
                        : erro.Trim('"'));

                model.Servicos = await _apiService.GetAuthenticatedAsync<List<FuncionarioServicoViewModel>>( $"api/FuncionarioServicos/funcionario/{model.FuncionarioId}")
                    ?? new List<FuncionarioServicoViewModel>();

                model.Servicos = model.Servicos
                    .Where(s => s.Ativo)
                    .OrderBy(s => s.ServicoNome)
                    .ToList();

                return View(model);
            }

            TempData["SuccessMessage"] = "Marcação alterada com sucesso.";

            return RedirectToAction( nameof(Detalhes), new { id = model.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError( string.Empty, "Ocorreu um erro ao alterar a marcação.");

            model.Servicos = await _apiService.GetAuthenticatedAsync<List<FuncionarioServicoViewModel>>($"api/FuncionarioServicos/funcionario/{model.FuncionarioId}")
                ?? new List<FuncionarioServicoViewModel>();

            model.Servicos = model.Servicos
                .Where(s => s.Ativo)
                .OrderBy(s => s.ServicoNome)
                .ToList();

            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> HorariosDisponiveis( int funcionarioId, int servicoId, DateTime data)
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
        catch (Exception)
        {
            return Json(new List<DateTime>());
        }
    }

}
