using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Cliente;

[Authorize(Roles = "Cliente")]
public class FeedbackClienteController : Controller
{
    private readonly ApiService _apiService;

    public FeedbackClienteController(ApiService apiService)
    {
        _apiService = apiService;
    }


    // =========================
    // AVALIAR
    // =========================

    [HttpGet]
    public async Task<IActionResult> Avaliar(int marcacaoId)
    {
        if (marcacaoId <= 0)
        {
            return BadRequest();
        }

        try
        {
            var marcacao = await _apiService.GetAuthenticatedAsync<MarcacaoClienteViewModel>($"api/Marcacoes/{marcacaoId}");

            if (marcacao == null)
            {
                return NotFound();
            }

            if (!string.Equals( marcacao.EstadoMarcacaoNome, "Concluida", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Apenas marcações concluídas podem ser avaliadas.";

                return RedirectToAction( "Detalhes", "MarcacoesCliente",  new { id = marcacaoId });
            }

            // Verifica se esta marcação já possui feedback.
            try
            {
                var feedbackExistente = await _apiService.GetAuthenticatedAsync<FeedbackViewModel>( $"api/Feedbacks/marcacao/{marcacaoId}");

                if (feedbackExistente != null)
                {
                    return RedirectToAction(nameof(Detalhes), new { id = feedbackExistente.Id });
                }
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // 404 significa simplesmente que a marcação
                // ainda não possui feedback.
            }

            var model = new FeedbackClienteViewModel
            {
                MarcacaoId = marcacao.Id,
                ServicoNome = marcacao.ServicoNome,
                FuncionarioNome = marcacao.FuncionarioNome,
                DataHoraInicio = marcacao.DataHoraInicio
            };

            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível preparar a avaliação.";

            return RedirectToAction( "Detalhes", "MarcacoesCliente", new { id = marcacaoId });
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Avaliar( FeedbackClienteViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CarregarDadosMarcacao(model);

            return View(model);
        }

        try
        {
            var request = new
            {
                model.MarcacaoId,
                model.Classificacao,
                model.Comentario
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Post, "api/Feedbacks", request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "A sua avaliação foi enviada com sucesso.";

                return RedirectToAction( "Detalhes", "MarcacoesCliente", new { id = model.MarcacaoId });
            }

            var mensagem = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(mensagem)
                    ? "Não foi possível enviar a avaliação."
                    : mensagem);
        }
        catch
        {
            ModelState.AddModelError( string.Empty, "Não foi possível enviar a avaliação.");
        }

        await CarregarDadosMarcacao(model);

        return View(model);
    }


    // =========================
    // DETALHES
    // =========================

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var feedback = await _apiService.GetAuthenticatedAsync<FeedbackViewModel>( $"api/Feedbacks/{id}");

            if (feedback == null)
            {
                return NotFound();
            }

            var marcacao = await _apiService.GetAuthenticatedAsync<MarcacaoClienteViewModel>($"api/Marcacoes/{feedback.MarcacaoId}");

            if (marcacao == null)
            {
                return NotFound();
            }

            var model = new FeedbackClienteViewModel
            {
                Id = feedback.Id,
                MarcacaoId = feedback.MarcacaoId,

                ServicoNome = marcacao.ServicoNome,
                FuncionarioNome = feedback.FuncionarioNome,
                DataHoraInicio = marcacao.DataHoraInicio,

                Classificacao = feedback.Classificacao,
                Comentario = feedback.Comentario,
                DataCriacao = feedback.DataCriacao
            };

            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar a avaliação.";

            return RedirectToAction("Index", "MarcacoesCliente");
        }
    }


    // =========================
    // EDITAR
    // =========================

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var feedback = await _apiService.GetAuthenticatedAsync<FeedbackViewModel>($"api/Feedbacks/{id}");

            if (feedback == null)
            {
                return NotFound();
            }

            var marcacao = await _apiService.GetAuthenticatedAsync<MarcacaoClienteViewModel>( $"api/Marcacoes/{feedback.MarcacaoId}");

            if (marcacao == null)
            {
                return NotFound();
            }

            var model = new FeedbackClienteViewModel
            {
                Id = feedback.Id,
                MarcacaoId = feedback.MarcacaoId,

                ServicoNome = marcacao.ServicoNome,
                FuncionarioNome = feedback.FuncionarioNome,
                DataHoraInicio = marcacao.DataHoraInicio,

                Classificacao = feedback.Classificacao,
                Comentario = feedback.Comentario,
                DataCriacao = feedback.DataCriacao
            };

            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar a avaliação.";

            return RedirectToAction( "Index", "MarcacoesCliente");
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar( int id, FeedbackClienteViewModel model)
    {
        if (!model.Id.HasValue || id != model.Id.Value)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await CarregarDadosMarcacao(model);

            return View(model);
        }

        try
        {
            var request = new
            {
                model.Classificacao,
                model.Comentario
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Put, $"api/Feedbacks/{id}", request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "A sua avaliação foi atualizada com sucesso.";

                return RedirectToAction( nameof(Detalhes), new { id });
            }

            var mensagem = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(mensagem)
                    ? "Não foi possível atualizar a avaliação."
                    : mensagem);
        }
        catch
        {
            ModelState.AddModelError( string.Empty, "Não foi possível atualizar a avaliação.");
        }

        await CarregarDadosMarcacao(model);

        return View(model);
    }


    // =========================
    // ELIMINAR
    // =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar( int id, int marcacaoId)
    {
        if (id <= 0 || marcacaoId <= 0)
        {
            return BadRequest();
        }

        try
        {
            using var response = await _apiService.SendAuthenticatedJsonAsync<object>( HttpMethod.Delete, $"api/Feedbacks/{id}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "A sua avaliação foi eliminada.";

                return RedirectToAction( "Detalhes", "MarcacoesCliente", new { id = marcacaoId });
            }

            TempData["ErrorMessage"] = "Não foi possível eliminar a avaliação.";
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível eliminar a avaliação.";
        }

        return RedirectToAction( nameof(Detalhes), new { id });
    }


    // =========================
    // AUXILIAR
    // =========================

    private async Task CarregarDadosMarcacao( FeedbackClienteViewModel model)
    {
        try
        {
            var marcacao = await _apiService.GetAuthenticatedAsync<MarcacaoClienteViewModel>( $"api/Marcacoes/{model.MarcacaoId}");

            if (marcacao == null)
            {
                return;
            }

            model.ServicoNome = marcacao.ServicoNome;
            model.FuncionarioNome = marcacao.FuncionarioNome;
            model.DataHoraInicio = marcacao.DataHoraInicio;
        }
        catch
        {
            // Mantém os restantes dados introduzidos pelo Cliente.
        }
    }
}
