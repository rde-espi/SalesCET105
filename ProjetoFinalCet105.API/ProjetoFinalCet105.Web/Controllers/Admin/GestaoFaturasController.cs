using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

[Authorize]
public class GestaoFaturasController : Controller
{
    private readonly ApiService _apiService;

    public GestaoFaturasController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var faturas = await _apiService.GetAuthenticatedAsync<List<FaturaViewModel>>("api/Faturas") ?? new List<FaturaViewModel>();

            var model = new GestaoFaturasViewModel
            {
                Faturas = faturas
                    .OrderByDescending(f => f.DataEmissao)
                    .ToList(),

                TotalFaturas = faturas.Count,

                Emitidas = faturas.Count(f =>
                    !string.Equals(
                        f.Estado,
                        "Anulada",
                        StringComparison.OrdinalIgnoreCase)),

                Anuladas = faturas.Count(f =>
                    string.Equals(
                        f.Estado,
                        "Anulada",
                        StringComparison.OrdinalIgnoreCase)),

                TotalFaturado = faturas
                    .Where(f =>
                        !string.Equals(
                            f.Estado,
                            "Anulada",
                            StringComparison.OrdinalIgnoreCase))
                    .Sum(f => f.Total)
            };

            return View(model);
        }
        catch (Exception)
        {
            return View(new GestaoFaturasViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        try
        {
            var fatura = await _apiService.GetAuthenticatedAsync<FaturaViewModel>($"api/Faturas/{id}");

            if (fatura == null)
            {
                return NotFound();
            }

            return View(fatura);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Pdf(int id)
    {
        try
        {
            var response = await _apiService.SendAuthenticatedJsonAsync<object>(HttpMethod.Get, $"api/Faturas/{id}/pdf", null);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = await ObterMensagemErroApi(response);

                return RedirectToAction(nameof(Index));
            }

            var pdf = await response.Content.ReadAsByteArrayAsync();

            var fatura = await _apiService.GetAuthenticatedAsync<FaturaViewModel>($"api/Faturas/{id}");

            var nomeFicheiro = string.IsNullOrWhiteSpace(fatura?.Numero)
                ? $"Fatura_{id}.pdf"
                : $"Fatura_{fatura.Numero.Replace("/", "_")}.pdf";

            return File(pdf, "application/pdf", nomeFicheiro);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível obter o PDF da fatura.";

            return RedirectToAction(nameof(Index));
        }
    }

    private static async Task<string> ObterMensagemErroApi(HttpResponseMessage response)
    {
        try
        {
            var json =
                await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            if (json != null)
            {
                if (json.TryGetValue("erro", out var erro) && erro != null)
                {
                    return erro.ToString()!;
                }

                if (json.TryGetValue("message", out var message) && message != null)
                {
                    return message.ToString()!;
                }

                if (json.TryGetValue("title", out var title) && title != null)
                {
                    return title.ToString()!;
                }
            }
        }
        catch
        {
        }

        return "Não foi possível obter o documento.";
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        try
        {
            var response = await _apiService.SendAuthenticatedJsonAsync<object>(HttpMethod.Patch, $"api/Faturas/{id}/anular", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Fatura anulada com sucesso.";

                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = await ObterMensagemErroApi(response);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível anular a fatura.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> PorFaturar()
    {
        try
        {
            var marcacoes =
                await _apiService.GetAuthenticatedAsync<List<MarcacaoClienteViewModel>>(
                    "api/Marcacoes")
                ?? new List<MarcacaoClienteViewModel>();

            var faturas =
                await _apiService.GetAuthenticatedAsync<List<FaturaViewModel>>(
                    "api/Faturas")
                ?? new List<FaturaViewModel>();

            var marcacoesFaturadas = faturas
                .Select(f => f.MarcacaoId)
                .ToHashSet();

            var porFaturar = marcacoes
                .Where(m =>
                    string.Equals(
                        m.EstadoMarcacaoNome,
                        "Concluida",
                        StringComparison.OrdinalIgnoreCase)
                    && !marcacoesFaturadas.Contains(m.Id))
                .OrderByDescending(m => m.DataHoraInicio)
                .ToList();

            return View(porFaturar);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] =
                "Não foi possível carregar as marcações por faturar.";

            return View(new List<MarcacaoClienteViewModel>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Emitir(int marcacaoId)
    {
        try
        {
            var response = await _apiService.SendAuthenticatedJsonAsync<object>(HttpMethod.Post, $"api/Faturas/marcacao/{marcacaoId}", null);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Fatura emitida com sucesso.";

                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = await ObterMensagemErroApi(response);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível emitir a fatura.";
        }

        return RedirectToAction(nameof(PorFaturar));
    }
}