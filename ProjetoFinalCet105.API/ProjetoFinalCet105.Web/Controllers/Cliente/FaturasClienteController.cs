using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Cliente;

[Authorize(Roles = "Cliente")]
public class FaturasClienteController : Controller
{
    private readonly ApiService _apiService;

    public FaturasClienteController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    [Route("FaturasCliente/Index")]
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
        catch
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
        catch
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

            var nomeFicheiro =
                string.IsNullOrWhiteSpace(fatura?.Numero)
                    ? $"Fatura_{id}.pdf"
                    : $"Fatura_{fatura.Numero.Replace("/", "_")}.pdf";

            return File(pdf, "application/pdf", nomeFicheiro);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível obter o PDF da fatura.";

            return RedirectToAction(nameof(Index));
        }
    }

    private static async Task<string> ObterMensagemErroApi(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            if (json != null)
            {
                if (json.TryGetValue("erro", out var erro) &&
                    erro != null)
                {
                    return erro.ToString()!;
                }

                if (json.TryGetValue("message", out var message) &&
                    message != null)
                {
                    return message.ToString()!;
                }

                if (json.TryGetValue("title", out var title) &&
                    title != null)
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
}