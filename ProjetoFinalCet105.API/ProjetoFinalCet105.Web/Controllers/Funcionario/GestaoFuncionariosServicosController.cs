using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Funcionario;

[Authorize]
public class GestaoFuncionarioServicosController : Controller
{
    private readonly ApiService _apiService;

    public GestaoFuncionarioServicosController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var funcionarios = await _apiService.GetAuthenticatedAsync<List<FuncionarioViewModel>>("api/Funcionarios") ?? new List<FuncionarioViewModel>();

        return View(funcionarios
            .Where(f => f.Ativo)
            .OrderBy(f => f.NomeCompleto)
            .ToList());
    }
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var funcionario = await _apiService.GetAuthenticatedAsync<FuncionarioViewModel>($"api/Funcionarios/{id}");

        if (funcionario == null)
        {
            return NotFound();
        }

        var servicos = await _apiService.GetAuthenticatedAsync<List<ServicoViewModel>>("api/Servicos") ?? new List<ServicoViewModel>();
        var associacoes = await _apiService.GetAuthenticatedAsync<List<FuncionarioServicoViewModel>>($"api/FuncionarioServicos/funcionario/{id}") ?? new List<FuncionarioServicoViewModel>();

        var model = new FuncionarioFormViewModel
        {
            Id = funcionario.Id,
            NomeCompleto = funcionario.NomeCompleto,

            Servicos = servicos
                .Where(s =>
                    s.Disponivel ||
                    associacoes.Any(a => a.ServicoId == s.Id))
                .OrderBy(s => s.CategoriaNome)
                .ThenBy(s => s.Nome)
                .Select(s =>
                {
                    var associacao =
                        associacoes.FirstOrDefault(
                            a => a.ServicoId == s.Id);

                    return new FuncionarioServicoSelecaoViewModel
                    {
                        ServicoId = s.Id,
                        Nome = s.Nome,
                        CategoriaNome = s.CategoriaNome,
                        Selecionado = associacao?.Ativo == true,
                        FuncionarioServicoId = associacao?.Id
                    };
                })
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, FuncionarioFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var atualizado = await AtualizarServicosFuncionarioAsync(model.Id, model.Servicos);

        if (!atualizado)
        {
            TempData["ErrorMessage"] = "Não foi possível atualizar os serviços do profissional.";

            return RedirectToAction(nameof(Editar), new { id = model.Id });
        }

        TempData["SuccessMessage"] = "Serviços do profissional atualizados com sucesso.";

        return RedirectToAction(nameof(Editar), new { id = model.Id });
    }

    private async Task<bool> AtualizarServicosFuncionarioAsync(int funcionarioId, IEnumerable<FuncionarioServicoSelecaoViewModel> servicos)
    {
        foreach (var servico in servicos)
        {
            // Nunca existiu associação e foi selecionado -> CRIAR
            if (servico.Selecionado &&
                !servico.FuncionarioServicoId.HasValue)
            {
                var dto = new FuncionarioServicoRequestViewModel
                {
                    FuncionarioId = funcionarioId,
                    ServicoId = servico.ServicoId,
                    Ativo = true
                };

                using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Post, "api/FuncionarioServicos", dto);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                continue;
            }

            // Nunca existiu e continua não selecionado
            if (!servico.FuncionarioServicoId.HasValue)
            {
                continue;
            }

            var associacao = await _apiService.GetAuthenticatedAsync<FuncionarioServicoViewModel>($"api/FuncionarioServicos/{servico.FuncionarioServicoId.Value}");

            if (associacao == null)
            {
                return false;
            }

            // Associação existia, estava inativa e foi novamente selecionada
            if (servico.Selecionado && !associacao.Ativo)
            {
                var dto = new FuncionarioServicoRequestViewModel
                {
                    Id = associacao.Id,
                    FuncionarioId = associacao.FuncionarioId,
                    ServicoId = associacao.ServicoId,
                    PrecoPersonalizado = associacao.PrecoPersonalizado,
                    DuracaoPersonalizadaMinutos =
                        associacao.DuracaoPersonalizadaMinutos,
                    Ativo = true
                };

                using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Put, $"api/FuncionarioServicos/{associacao.Id}", dto);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                continue;
            }

            // Associação ativa foi desmarcada
            if (!servico.Selecionado && associacao.Ativo)
            {
                using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Delete, $"api/FuncionarioServicos/{associacao.Id}");

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
            }
        }

        return true;
    }
}