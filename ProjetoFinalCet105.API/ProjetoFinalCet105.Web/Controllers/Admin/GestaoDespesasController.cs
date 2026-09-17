using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

[Authorize]
public class GestaoDespesasController : Controller
{
    private readonly ApiService _apiService;

    public GestaoDespesasController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var despesas = await _apiService.GetAuthenticatedAsync<List<DespesaViewModel>>( "api/Despesas") ?? new List<DespesaViewModel>();

            despesas = despesas
                .OrderByDescending(d => d.DataDespesa)
                .ThenByDescending(d => d.Id)
                .ToList();

            var hoje = DateTime.Today;

            var model = new GestaoDespesasViewModel
            {
                Despesas = despesas,

                TotalRegistos = despesas.Count,

                TotalDespesas = despesas.Sum(d => d.Valor),

                TotalMes = despesas
                    .Where(d =>
                        d.DataDespesa.Year == hoje.Year &&
                        d.DataDespesa.Month == hoje.Month)
                    .Sum(d => d.Valor),

                TotalAno = despesas
                    .Where(d => d.DataDespesa.Year == hoje.Year)
                    .Sum(d => d.Valor)
            };

            return View(model);
        }
        catch (Exception)
        {
            return View(new GestaoDespesasViewModel());
        }
    }

    [HttpGet]
    public IActionResult Criar()
    {
        return View(new DespesaViewModel
        {
            DataDespesa = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(DespesaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Post,"api/Despesas", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Despesa registada com sucesso.";

                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError( string.Empty, await ObterMensagemErroApi(response));
        }
        catch (Exception)
        {
            ModelState.AddModelError( string.Empty, "Não foi possível registar a despesa.");
        }

        return View(model);
    }

    private static async Task<string> ObterMensagemErroApi(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            if (json != null)
            {
                if (json.TryGetValue("erro", out var erro) &&  erro != null)
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

        return "Não foi possível concluir a operação.";
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        try
        {
            var despesa = await _apiService.GetAuthenticatedAsync<DespesaViewModel>($"api/Despesas/{id}");

            if (despesa == null)
            {
                return NotFound();
            }

            return View(despesa);
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
            var despesa =
                await _apiService.GetAuthenticatedAsync<DespesaViewModel>(
                    $"api/Despesas/{id}");

            if (despesa == null)
            {
                return NotFound();
            }

            return View(despesa);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, DespesaViewModel model)
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
            var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Put, $"api/Despesas/{id}", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Despesa atualizada com sucesso.";

                return RedirectToAction( nameof(Detalhes), new { id });
            }

            ModelState.AddModelError(string.Empty, await ObterMensagemErroApi(response));
        }
        catch (Exception)
        {
            ModelState.AddModelError( string.Empty, "Não foi possível atualizar a despesa.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var response = await _apiService.SendAuthenticatedJsonAsync<object>( HttpMethod.Delete, $"api/Despesas/{id}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Despesa eliminada com sucesso.";

                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = await ObterMensagemErroApi(response);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível eliminar a despesa.";
        }

        return RedirectToAction( nameof(Detalhes), new { id });
    }
}
