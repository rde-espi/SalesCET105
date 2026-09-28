using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Publico;

public class ProfissionaisController : Controller
{
    private readonly ApiService _apiService;

    public ProfissionaisController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var profissionais = await _apiService.GetAsync<List<FuncionarioViewModel>>( "api/Funcionarios");

            profissionais ??= new List<FuncionarioViewModel>();

            profissionais = profissionais
                .Where(f => f.Ativo && f.Disponivel)
                .OrderBy(f => f.NomeCompleto)
                .ToList();

            var profissionaisPublicos = new List<FuncionarioMarcacaoClienteViewModel>();

            foreach (var funcionario in profissionais)
            {
                var resumo = await _apiService.GetAsync<FeedbackResumoViewModel>( $"api/Feedbacks/funcionario/{funcionario.Id}/resumo");

                profissionaisPublicos.Add(new FuncionarioMarcacaoClienteViewModel
                {
                    Id = funcionario.Id,
                    NomeCompleto = funcionario.NomeCompleto,
                    Biografia = funcionario.Biografia,

                    MediaAvaliacao = resumo?.Media ?? 0,
                    TotalAvaliacoes = resumo?.TotalAvaliacoes ?? 0,

                    FotografiaUrl = Url.Action( nameof(Fotografia), "Profissionais", new { id = funcionario.Id })
                });
            }

            return View(profissionaisPublicos);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os profissionais.";

            return View(new List<FuncionarioViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var funcionarios = await _apiService.GetAsync<List<FuncionarioViewModel>>( "api/Funcionarios");

            var funcionario = funcionarios?
                .FirstOrDefault(f =>
                    f.Id == id &&
                    f.Ativo &&
                    f.Disponivel);

            if (funcionario == null)
            {
                return NotFound();
            }

            var resumo = await _apiService.GetAsync<FeedbackResumoViewModel>( $"api/Feedbacks/funcionario/{funcionario.Id}/resumo");

            var profissional = new FuncionarioMarcacaoClienteViewModel
            {
                Id = funcionario.Id,
                NomeCompleto = funcionario.NomeCompleto,
                Biografia = funcionario.Biografia,

                MediaAvaliacao = resumo?.Media ?? 0,
                TotalAvaliacoes = resumo?.TotalAvaliacoes ?? 0,

                FotografiaUrl = Url.Action( nameof(Fotografia), "Profissionais", new { id = funcionario.Id })
            };


            var servicos = await _apiService.GetAsync<List<FuncionarioServicoViewModel>>( $"api/FuncionarioServicos/funcionario/{id}");

            servicos ??= new List<FuncionarioServicoViewModel>();

            servicos = servicos
                .Where(s => s.Ativo)
                .OrderBy(s => s.ServicoNome)
                .ToList();

            ViewBag.Servicos = servicos;

            return View(profissional);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar o profissional.";

            return RedirectToAction(nameof(Index));
        }
    }


    [HttpGet]
    public async Task<IActionResult> Fotografia(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        using var response = await _apiService.GetResponseAsync( $"api/Funcionarios/{id}/fotografia");

        if (response == null || !response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();

        if (bytes.Length == 0)
        {
            return NotFound();
        }

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

        return File(bytes, contentType);
    }
}