using System.Globalization;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

public class GestaoServicosController : Controller
{
    private readonly ApiService _apiService;

    public GestaoServicosController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        var servicos = await _apiService.GetAuthenticatedAsync<List<ServicoViewModel>>("api/Servicos");

        return View(servicos ?? new List<ServicoViewModel>());
    }

    [HttpGet]
    public async Task<IActionResult> Imagem(int id)
    {
        var response = await _apiService.GetResponseAsync($"api/Servicos/{id}/imagem");

        if (response == null || !response.IsSuccessStatusCode)
        {
            response?.Dispose();
            return NotFound();
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

        response.Dispose();

        return File(bytes, contentType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desativar(int id)
    {
        var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Delete, $"api/Servicos/{id}");

        if (!response.IsSuccessStatusCode)
        {
            response.Dispose();
            return BadRequest(new { mensagem = "Não foi possível desativar o serviço." });
        }

        response.Dispose();

        return Ok(new { mensagem = "Serviço desativado com sucesso." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ativar(int id)
    {
        var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Patch, $"api/Servicos/{id}/ativar");

        if (!response.IsSuccessStatusCode)
        {
            response.Dispose();
            return BadRequest(new { mensagem = "Não foi possível reativar o serviço." });
        }

        response.Dispose();

        return Ok(new { mensagem = "Serviço reativado com sucesso." });
    }

    // =========================
    // CRIAR
    // =========================

    [HttpGet]
    public async Task<IActionResult> Criar()
    {
        var categorias = await _apiService.GetAuthenticatedAsync<List<CategoriaViewModel>>("api/Categorias");

        var model = new ServicoFormViewModel
        {
            Categorias = categorias?
                .Where(c => c.Ativa)
                .OrderBy(c => c.Nome)
                .ToList()
                ?? new List<CategoriaViewModel>()
        };

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(ServicoFormViewModel model)
    {
        var categorias = await _apiService.GetAuthenticatedAsync<List<CategoriaViewModel>>("api/Categorias");

        model.Categorias = categorias?
            .Where(c => c.Ativa)
            .OrderBy(c => c.Nome)
            .ToList()
            ?? new List<CategoriaViewModel>();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(model.CategoriaId.ToString()), "CategoriaId");
        content.Add(new StringContent(model.Nome), "Nome");

        if (!string.IsNullOrWhiteSpace(model.Descricao))
        {
            content.Add(new StringContent(model.Descricao), "Descricao");
        }

        content.Add(
            new StringContent(model.Preco.ToString(CultureInfo.InvariantCulture)),
            "Preco");

        content.Add(
            new StringContent(model.DuracaoMinutos.ToString()),
            "DuracaoMinutos");

        if (model.Imagem != null && model.Imagem.Length > 0)
        {
            var streamContent = new StreamContent(model.Imagem.OpenReadStream());

            streamContent.Headers.ContentType =
                new MediaTypeHeaderValue(model.Imagem.ContentType);

            content.Add(
                streamContent,
                "Imagem",
                model.Imagem.FileName);
        }

        using var response =
            await _apiService.SendAuthenticatedMultipartAsync(
                HttpMethod.Post,
                "api/Servicos",
                content);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                string.Empty,
                "Não foi possível criar o serviço.");

            return View(model);
        }

        TempData["Sucesso"] =
            "Serviço criado com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // EDITAR
    // =========================

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var servico = await _apiService.GetAuthenticatedAsync<ServicoViewModel>($"api/Servicos/{id}");

        if (servico == null)
        {
            return NotFound();
        }

        var categorias = await _apiService.GetAuthenticatedAsync<List<CategoriaViewModel>>("api/Categorias");

        var categoriasDisponiveis = categorias?
            .Where(c => c.Ativa || c.Id == servico.CategoriaId)
            .OrderBy(c => c.Nome)
            .ToList()
            ?? new List<CategoriaViewModel>();

        var imagemResponse = await _apiService.GetResponseAsync($"api/Servicos/{id}/imagem");
        var temImagem = imagemResponse?.IsSuccessStatusCode == true;
        imagemResponse?.Dispose();

        var model = new ServicoFormViewModel
        {
            Id = servico.Id,
            CategoriaId = servico.CategoriaId,
            Nome = servico.Nome,
            Descricao = servico.Descricao,
            Preco = servico.Preco,
            DuracaoMinutos = servico.DuracaoMinutos,
            TemImagem = temImagem,
            Categorias = categoriasDisponiveis
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ServicoFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var categorias = await _apiService.GetAuthenticatedAsync<List<CategoriaViewModel>>("api/Categorias");

        model.Categorias = categorias?
            .Where(c => c.Ativa || c.Id == model.CategoriaId)
            .OrderBy(c => c.Nome)
            .ToList()
            ?? new List<CategoriaViewModel>();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(model.CategoriaId.ToString()), "CategoriaId");
        content.Add(new StringContent(model.Nome), "Nome");

        if (!string.IsNullOrWhiteSpace(model.Descricao))
        {
            content.Add(new StringContent(model.Descricao), "Descricao");
        }

        content.Add(new StringContent(model.Preco.ToString(CultureInfo.InvariantCulture)), "Preco");
        content.Add(new StringContent(model.DuracaoMinutos.ToString()), "DuracaoMinutos");

        if (model.Imagem != null && model.Imagem.Length > 0)
        {
            var streamContent = new StreamContent(model.Imagem.OpenReadStream());

            streamContent.Headers.ContentType =
                new MediaTypeHeaderValue(model.Imagem.ContentType);

            content.Add(streamContent, "Imagem", model.Imagem.FileName);
        }

        using var response = await _apiService.SendAuthenticatedMultipartAsync(
            HttpMethod.Put,
            $"api/Servicos/{id}",
            content);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Não foi possível atualizar o serviço.");
            return View(model);
        }

        TempData["Sucesso"] = "Serviço atualizado com sucesso.";

        return RedirectToAction(nameof(Index));
    }
}