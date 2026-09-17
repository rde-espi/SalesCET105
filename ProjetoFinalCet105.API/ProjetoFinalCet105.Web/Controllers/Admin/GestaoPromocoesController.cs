using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

[Authorize]
public class GestaoPromocoesController : Controller
{
    private readonly ApiService _apiService;

    public GestaoPromocoesController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var promocoes = await _apiService.GetAuthenticatedAsync<List<PromoCodeViewModel>>("api/PromoCodes") ?? new List<PromoCodeViewModel>();

            promocoes = promocoes
                .OrderByDescending(p => p.DataInicio)
                .ThenByDescending(p => p.Id)
                .ToList();

            var agora = DateTime.Now;

            var model = new GestaoPromocoesViewModel
            {
                Promocoes = promocoes,
                Total = promocoes.Count,

                Ativas = promocoes.Count(p =>
                    p.Ativo &&
                    p.DataInicio <= agora &&
                    p.DataFim >= agora),

                Expiradas = promocoes.Count(p =>
                    p.DataFim < agora),

                Utilizacoes = promocoes.Sum(p =>
                    p.NumeroUtilizacoes)
            };

            return View(model);
        }
        catch (Exception)
        {
            return View(new GestaoPromocoesViewModel());
        }
    }

    [HttpGet]
    public IActionResult Criar()
    {
        return View(new PromoCodeViewModel
        {
            DataInicio = DateTime.Today,
            DataFim = DateTime.Today.AddMonths(1)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(PromoCodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Post, "api/PromoCodes", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Promoção criada com sucesso.";

                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, await ObterMensagemErroApi(response));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Não foi possível criar a promoção.");
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        try
        {
            var promocao = await _apiService.GetAuthenticatedAsync<PromoCodeViewModel>($"api/PromoCodes/{id}");

            if (promocao == null)
            {
                return NotFound();
            }

            return View(promocao);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var promocao =
                await _apiService.GetAuthenticatedAsync<PromoCodeViewModel>(
                    $"api/PromoCodes/{id}");

            if (promocao == null)
            {
                return NotFound();
            }

            return View(promocao);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, PromoCodeViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Put, $"api/PromoCodes/{id}", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Promoção atualizada com sucesso.";

                return RedirectToAction(nameof(Detalhes), new { id });
            }

            ModelState.AddModelError(string.Empty, await ObterMensagemErroApi(response));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Não foi possível atualizar a promoção.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarEstado(int id, bool ativoAtual)
    {
        try
        {
            var request = new
            {
                Ativo = !ativoAtual
            };

            var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Patch, $"api/PromoCodes/{id}/ativo", request);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] =
                    !ativoAtual
                        ? "Promoção ativada com sucesso."
                        : "Promoção desativada com sucesso.";
            }
            else
            {
                TempData["ErrorMessage"] = await ObterMensagemErroApi(response);
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível alterar o estado da promoção.";
        }

        return RedirectToAction(nameof(Detalhes), new { id });
    }


    private static async Task<string> ObterMensagemErroApi(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            if (json != null)
            {
                if (json.TryGetValue("erro", out var erro) && erro != null)
                    return erro.ToString()!;

                if (json.TryGetValue("message", out var message) && message != null)
                    return message.ToString()!;

                if (json.TryGetValue("title", out var title) && title != null)
                    return title.ToString()!;
            }
        }
        catch
        {
        }

        return "Não foi possível concluir a operação.";
    }
}