using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Cliente;

[Authorize(Roles = "Cliente")]
public class ExplorarServicosController : Controller
{
    private readonly ApiService _apiService;

    public ExplorarServicosController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var servicos = await _apiService.GetAuthenticatedAsync<List<ServicoViewModel>>("api/Servicos") ?? new List<ServicoViewModel>();

            var categorias = await _apiService.GetAuthenticatedAsync<List<CategoriaViewModel>>("api/Categorias") ?? new List<CategoriaViewModel>();

            servicos = servicos
                .Where(s => s.Disponivel)
                .OrderBy(s => s.Nome)
                .ToList();

            categorias = categorias
                .OrderBy(c => c.Nome)
                .ToList();

            ViewBag.Categorias = categorias;

            return View(servicos);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os serviços.";

            ViewBag.Categorias = new List<CategoriaViewModel>();

            return View(new List<ServicoViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> ImagemServico(int id)
    {
        if (id <= 0)
            return NotFound();

        using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Get, $"api/Servicos/{id}/imagem");

        if (!response.IsSuccessStatusCode)
            return NotFound();

        var bytes = await response.Content.ReadAsByteArrayAsync();

        if (bytes.Length == 0)
            return NotFound();

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

        return File(bytes, contentType);
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        if (id <= 0)
            return BadRequest();

        try
        {
            var servico = await _apiService.GetAuthenticatedAsync<ServicoViewModel>($"api/Servicos/{id}");

            if (servico == null || !servico.Disponivel)
                return NotFound();

            var funcionarios = await _apiService.GetAuthenticatedAsync<List<FuncionarioViewModel>>($"api/Funcionarios/servico/{id}") ?? new List<FuncionarioViewModel>();

            var profissionais = new List<FuncionarioMarcacaoClienteViewModel>();

            foreach (var funcionario in funcionarios.OrderBy(f => f.NomeCompleto))
            {
                FeedbackResumoViewModel? resumo = null;

                try
                {
                    resumo = await _apiService.GetAuthenticatedAsync<FeedbackResumoViewModel>($"api/Feedbacks/funcionario/{funcionario.Id}/resumo");
                }
                catch
                {

                }

                profissionais.Add(
                    new FuncionarioMarcacaoClienteViewModel
                    {
                        Id = funcionario.Id,
                        NomeCompleto = funcionario.NomeCompleto,
                        Biografia = funcionario.Biografia,

                        MediaAvaliacao = resumo?.Media ?? 0,
                        TotalAvaliacoes = resumo?.TotalAvaliacoes ?? 0,

                        FotografiaUrl = Url.Action(
                            nameof(FotografiaFuncionario),
                            "ExplorarServicos",
                            new { id = funcionario.Id })
                    });
            }

            ViewBag.Profissionais = profissionais;

            return View(servico);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os detalhes do serviço.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> FotografiaFuncionario(int id)
    {
        if (id <= 0)
            return NotFound();

        using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Get, $"api/Funcionarios/{id}/fotografia");

        if (!response.IsSuccessStatusCode)
            return NotFound();

        var bytes = await response.Content.ReadAsByteArrayAsync();

        if (bytes.Length == 0)
            return NotFound();

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

        return File(bytes, contentType);
    }
}
