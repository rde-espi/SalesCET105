using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class PesquisaController : Controller
{
    private readonly ApiService _apiService;

    public PesquisaController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var model = new PesquisaGlobalViewModel
        {
            Termo = q?.Trim() ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(model.Termo))
            return View(model);

        try
        {
            var clientes = await _apiService.GetAuthenticatedAsync<List<ClienteViewModel>>("api/Clientes")?? new List<ClienteViewModel>();

            var servicos = await _apiService.GetAuthenticatedAsync<List<ServicoViewModel>>("api/Servicos")?? new List<ServicoViewModel>();

            var marcacoes = await _apiService.GetAuthenticatedAsync<List<MarcacaoClienteViewModel>>("api/Marcacoes") ?? new List<MarcacaoClienteViewModel>();

            var funcionarios = await _apiService.GetAuthenticatedAsync<List<FuncionarioViewModel>>("api/Funcionarios") ?? new List<FuncionarioViewModel>();

            var termo = model.Termo;

            model.Clientes = clientes
                .Where(c =>
                    Contem(c.NomeCompleto, termo) ||
                    Contem(c.Email, termo) ||
                    Contem(c.Telefone, termo) ||
                    Contem(c.Contribuinte, termo))
                .OrderBy(c => c.NomeCompleto)
                .Take(20)
                .ToList();

            model.Servicos = servicos
                .Where(s =>
                    Contem(s.Nome, termo) ||
                    Contem(s.CategoriaNome, termo) ||
                    Contem(s.Descricao, termo))
                .OrderBy(s => s.Nome)
                .Take(20)
                .ToList();

            model.Marcacoes = marcacoes
                .Where(m =>
                    Contem(m.ClienteNome, termo) ||
                    Contem(m.FuncionarioNome, termo) ||
                    Contem(m.ServicoNome, termo) ||
                    Contem(m.EstadoMarcacaoNome, termo) ||
                    m.Id.ToString() == termo)
                .OrderByDescending(m => m.DataHoraInicio)
                .Take(20)
                .ToList();

            model.Funcionarios = funcionarios
                .Where(f =>
                    Contem(f.NomeCompleto, termo) ||
                    Contem(f.Email, termo) ||
                    Contem(f.Telefone, termo))
                .OrderBy(f => f.NomeCompleto)
                .Take(20)
                .ToList();
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível realizar a pesquisa neste momento.";
        }

        return View(model);
    }

    private static bool Contem(string? valor, string termo)
    {
        return !string.IsNullOrWhiteSpace(valor) && valor.Contains( termo, StringComparison.OrdinalIgnoreCase);
    }
}