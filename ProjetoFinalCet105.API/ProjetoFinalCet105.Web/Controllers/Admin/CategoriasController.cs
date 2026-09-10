using System.Net.Http.Headers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class CategoriasController : Controller
{
    private readonly ApiService _apiService;

    public CategoriasController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        var categorias = await _apiService.GetAuthenticatedAsync<List<CategoriaViewModel>>("api/Categorias");

        return View(categorias ?? new List<CategoriaViewModel>());
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Imagem(int id)
    {
        var response =
            await _apiService
                .GetResponseAsync(
                    $"api/Categorias/{id}/imagem");

        if (response == null ||
            !response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var bytes =
            await response.Content
                .ReadAsByteArrayAsync();

        var contentType =
            response.Content.Headers.ContentType?
                .MediaType
            ?? "image/jpeg";

        return File(bytes, contentType);
    }


    // =========================
    // CRIAR
    // =========================

    [HttpGet]
    public IActionResult Create()
    {
        return View(
            new CategoriaFormViewModel());
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create( CategoriaFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        using var content =
            new MultipartFormDataContent();

        content.Add(
            new StringContent(model.Nome),
            "Nome");

        if (!string.IsNullOrWhiteSpace(model.Descricao))
        {
            content.Add(
                new StringContent(model.Descricao),
                "Descricao");
        }

        if (model.Imagem != null &&
            model.Imagem.Length > 0)
        {
            var streamContent =
                new StreamContent(
                    model.Imagem.OpenReadStream());

            streamContent.Headers.ContentType =
                new MediaTypeHeaderValue(
                    model.Imagem.ContentType);

            content.Add(
                streamContent,
                "Imagem",
                model.Imagem.FileName);
        }

        using var response =
            await _apiService
                .SendAuthenticatedMultipartAsync(
                    HttpMethod.Post,
                    "api/Categorias",
                    content);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                string.Empty,
                "Não foi possível criar a categoria.");

            return View(model);
        }

        TempData["Sucesso"] =
            "Categoria criada com sucesso.";

        return RedirectToAction(nameof(Index));
    }


    // =========================
    // EDITAR
    // =========================

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var categoria =
            await _apiService
                .GetAuthenticatedAsync<CategoriaViewModel>(
                    $"api/Categorias/{id}");

        if (categoria == null)
        {
            return NotFound();
        }

        var model =
            new CategoriaFormViewModel
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                Descricao = categoria.Descricao,
                TemImagem = true
            };

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        int id,
        CategoriaFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        using var content =
            new MultipartFormDataContent();

        content.Add(
            new StringContent(model.Nome),
            "Nome");

        if (!string.IsNullOrWhiteSpace(model.Descricao))
        {
            content.Add(
                new StringContent(model.Descricao),
                "Descricao");
        }

        if (model.Imagem != null &&
            model.Imagem.Length > 0)
        {
            var streamContent =
                new StreamContent(
                    model.Imagem.OpenReadStream());

            streamContent.Headers.ContentType =
                new MediaTypeHeaderValue(
                    model.Imagem.ContentType);

            content.Add(
                streamContent,
                "Imagem",
                model.Imagem.FileName);
        }

        using var response =
            await _apiService
                .SendAuthenticatedMultipartAsync(
                    HttpMethod.Put,
                    $"api/Categorias/{id}",
                    content);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                string.Empty,
                "Não foi possível atualizar a categoria.");

            return View(model);
        }

        TempData["Sucesso"] =
            "Categoria atualizada com sucesso.";

        return RedirectToAction(nameof(Index));
    }


    // =========================
    // DESATIVAR
    // =========================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desativar(int id)
    {
        var response =
            await _apiService
                .SendAuthenticatedAsync(
                    HttpMethod.Delete,
                    $"api/Categorias/{id}");

        if (response.IsSuccessStatusCode)
        {
            TempData["Sucesso"] =
                "Categoria desativada com sucesso.";
        }
        else
        {
            TempData["Erro"] =
                "Não foi possível desativar a categoria.";
        }

        response.Dispose();

        return RedirectToAction(nameof(Index));
    }
}