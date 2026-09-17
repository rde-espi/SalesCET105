using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

[Authorize]
public class GestaoIndisponibilidadesController : Controller
{
    private readonly ApiService _apiService;

    public GestaoIndisponibilidadesController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var indisponibilidades = await _apiService.GetAuthenticatedAsync<List<IndisponibilidadeViewModel>>("api/Indisponibilidades") ?? new List<IndisponibilidadeViewModel>();

            var funcionarios = await _apiService.GetAuthenticatedAsync<List<FuncionarioViewModel>>("api/Funcionarios") ?? new List<FuncionarioViewModel>();

            var hoje = DateTime.Today;
            var amanha = hoje.AddDays(1);

            var model = new GestaoIndisponibilidadesViewModel
            {
                Indisponibilidades = indisponibilidades
                    .OrderBy(i => i.DataHoraInicio)
                    .ToList(),

                Funcionarios = funcionarios
                    .Where(f => f.Ativo)
                    .OrderBy(f => f.NomeCompleto)
                    .ToList(),

                Total = indisponibilidades.Count,

                Hoje = indisponibilidades.Count(i =>
                    i.DataHoraInicio < amanha &&
                    i.DataHoraFim > hoje),

                Proximas = indisponibilidades.Count(i =>
                    i.DataHoraInicio >= amanha),

                DiasCompletos = indisponibilidades.Count(i =>
                    i.DiaCompleto)
            };

            return View(model);
        }
        catch (Exception)
        {
            return View(new GestaoIndisponibilidadesViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Criar()
    {
        try
        {
            var funcionarios = await _apiService.GetAuthenticatedAsync<List<FuncionarioViewModel>>("api/Funcionarios") ?? new List<FuncionarioViewModel>();

            var model = new CriarIndisponibilidadeViewModel
            {
                Data = DateTime.Today,

                Funcionarios = funcionarios
                    .Where(f => f.Ativo)
                    .OrderBy(f => f.NomeCompleto)
                    .ToList()
            };

            return View(model);
        }
        catch (Exception)
        {
            return View(new CriarIndisponibilidadeViewModel());
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarIndisponibilidadeViewModel model)
    {
        var funcionarios = await _apiService.GetAuthenticatedAsync<List<FuncionarioViewModel>>("api/Funcionarios") ?? new List<FuncionarioViewModel>();

        model.Funcionarios = funcionarios
            .Where(f => f.Ativo)
            .OrderBy(f => f.NomeCompleto)
            .ToList();

        if (!User.IsInRole("Admin"))
        {
            ModelState.Remove(nameof(model.FuncionarioId));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var diaCompleto = model.Tipo == "dia";
        var restoDoDia = model.Tipo == "resto";

        if (model.Tipo == "periodo")
        {
            if (!model.HoraInicio.HasValue || !model.HoraFim.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Indique a hora de início e a hora de fim.");

                return View(model);
            }

            if (model.HoraFim.Value <= model.HoraInicio.Value)
            {
                ModelState.AddModelError(string.Empty, "A hora de fim deve ser posterior à hora de início.");

                return View(model);
            }
        }

        var dataHoraInicio =
            model.Tipo == "periodo"
                ? model.Data.Date.Add(model.HoraInicio!.Value)
                : model.Data.Date;

        var dataHoraFim =
            model.Tipo == "periodo"
                ? model.Data.Date.Add(model.HoraFim!.Value)
                : model.Data.Date;

        var request = new NovaIndisponibilidadeRequest
        {
            FuncionarioId = model.FuncionarioId,
            DataHoraInicio = dataHoraInicio,
            DataHoraFim = dataHoraFim,
            Motivo = model.Motivo,
            DiaCompleto = diaCompleto,
            RestoDoDia = restoDoDia
        };

        var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Post, "api/Indisponibilidades", request);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Indisponibilidade criada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        var mensagem = await ObterMensagemErroApi(response);

        ModelState.AddModelError(string.Empty, mensagem);

        return View(model);
    }

    private static async Task<string> ObterMensagemErroApi(
    HttpResponseMessage response)
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

        return "Não foi possível criar a indisponibilidade.";
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        try
        {
            var indisponibilidade =
                await _apiService.GetAuthenticatedAsync<IndisponibilidadeViewModel>(
                    $"api/Indisponibilidades/{id}");

            if (indisponibilidade == null)
            {
                return NotFound();
            }

            var funcionarios =
                await _apiService.GetAuthenticatedAsync<List<FuncionarioViewModel>>(
                    "api/Funcionarios")
                ?? new List<FuncionarioViewModel>();

            var tipo =
                indisponibilidade.DiaCompleto
                    ? "dia"
                    : indisponibilidade.RestoDoDia
                        ? "resto"
                        : "periodo";

            var model = new EditarIndisponibilidadeViewModel
            {
                Id = indisponibilidade.Id,
                FuncionarioId = indisponibilidade.FuncionarioId,
                Data = indisponibilidade.DataHoraInicio.Date,
                Tipo = tipo,
                Motivo = indisponibilidade.Motivo,

                HoraInicio = tipo == "periodo"
                    ? indisponibilidade.DataHoraInicio.TimeOfDay
                    : null,

                HoraFim = tipo == "periodo"
                    ? indisponibilidade.DataHoraFim.TimeOfDay
                    : null,

                Funcionarios = funcionarios
                    .Where(f => f.Ativo ||
                                f.Id == indisponibilidade.FuncionarioId)
                    .OrderBy(f => f.NomeCompleto)
                    .ToList()
            };

            return View(model);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarIndisponibilidadeViewModel model)
    {
        var funcionarios = await _apiService.GetAuthenticatedAsync<List<FuncionarioViewModel>>("api/Funcionarios")
            ?? new List<FuncionarioViewModel>();

        model.Funcionarios = funcionarios
            .Where(f => f.Ativo ||
                        f.Id == model.FuncionarioId)
            .OrderBy(f => f.NomeCompleto)
            .ToList();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var diaCompleto = model.Tipo == "dia";
        var restoDoDia = model.Tipo == "resto";

        if (model.Tipo == "periodo")
        {
            if (!model.HoraInicio.HasValue || !model.HoraFim.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Indique a hora de início e a hora de fim.");

                return View(model);
            }

            if (model.HoraFim.Value <= model.HoraInicio.Value)
            {
                ModelState.AddModelError(string.Empty, "A hora de fim deve ser posterior à hora de início.");

                return View(model);
            }
        }

        var inicio =
            model.Tipo == "periodo"
                ? model.Data.Date.Add(model.HoraInicio!.Value)
                : model.Data.Date;

        var fim =
            model.Tipo == "periodo"
                ? model.Data.Date.Add(model.HoraFim!.Value)
                : model.Data.Date;

        var request = new UpdateIndisponibilidadeRequest
        {
            FuncionarioId = model.FuncionarioId,
            DataHoraInicio = inicio,
            DataHoraFim = fim,
            Motivo = model.Motivo,
            DiaCompleto = diaCompleto,
            RestoDoDia = restoDoDia
        };

        var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Put, $"api/Indisponibilidades/{model.Id}", request);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Indisponibilidade atualizada com sucesso.";

            return RedirectToAction(nameof(Index));
        }

        var mensagem = await ObterMensagemErroApi(response);

        ModelState.AddModelError(string.Empty, mensagem);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            var response = await _apiService.SendAuthenticatedJsonAsync<object>(HttpMethod.Delete, $"api/Indisponibilidades/{id}", null);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Indisponibilidade eliminada com sucesso.";

                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = await ObterMensagemErroApi(response);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível eliminar a indisponibilidade.";
        }

        return RedirectToAction(nameof(Index));
    }

}