using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class GestaoFuncionariosController : Controller
{
    private readonly ApiService _apiService;

    public GestaoFuncionariosController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var funcionarios = await _apiService.GetAuthenticatedAsync<List<FuncionarioViewModel>>("api/Funcionarios");

        return View(funcionarios ?? new List<FuncionarioViewModel>());
    }

    [HttpGet]
    public async Task<IActionResult> Fotografia(int id)
    {
        using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Get, $"api/Funcionarios/{id}/fotografia");

        if (!response.IsSuccessStatusCode)
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


    [HttpGet]
    public IActionResult Criar()
    {
        var model = new FuncionarioFormViewModel
        {
            Ativo = true,
            Disponivel = true,
            DataAdmissao = DateTime.Today
        };

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(FuncionarioFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(model.NomeCompleto), "NomeCompleto");
        content.Add(new StringContent(model.Email), "Email");

        if (!string.IsNullOrWhiteSpace(model.Telefone))
        {
            content.Add(new StringContent(model.Telefone), "Telefone");
        }

        if (!string.IsNullOrWhiteSpace(model.Biografia))
        {
            content.Add(new StringContent(model.Biografia), "Biografia");
        }

        if (model.DataAdmissao.HasValue)
        {
            content.Add(new StringContent(model.DataAdmissao.Value.ToString("yyyy-MM-dd")), "DataAdmissao");
        }

        content.Add(new StringContent(model.Disponivel.ToString().ToLowerInvariant()), "Disponivel");

        using var response = await _apiService.SendAuthenticatedMultipartAsync(HttpMethod.Post, "api/Funcionarios", content);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(erro)
                    ? "Não foi possível criar o funcionário."
                    : erro);

            return View(model);
        }

        TempData["SuccessMessage"] = "Funcionário criado com sucesso. O convite de primeiro acesso foi enviado por email.";

        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var funcionario = await _apiService.GetAuthenticatedAsync<FuncionarioViewModel>($"api/Funcionarios/{id}");

        if (funcionario == null)
        {
            return NotFound();
        }

        var permissoesTemporarias = await _apiService.GetAuthenticatedAsync<List<PermissaoAdminTemporariaViewModel>>("api/Admin/permissoes-temporarias");

        permissoesTemporarias ??= new List<PermissaoAdminTemporariaViewModel>();

        var permissaoAtiva = permissoesTemporarias
            .Where(p =>
            p.FuncionarioUserId == funcionario.UserId &&
            p.Estado == "Ativa")
            .OrderByDescending(p => p.DataFim)
            .FirstOrDefault();

        var servicos = await _apiService.GetAuthenticatedAsync<List<ServicoViewModel>>("api/Servicos");

        var associacoes = await _apiService.GetAuthenticatedAsync<List<FuncionarioServicoViewModel>>($"api/FuncionarioServicos/funcionario/{id}");

        var competencias = await _apiService.GetAuthenticatedAsync<List<CompetenciaViewModel>>("api/Competencias");

        var competenciasFuncionario = await _apiService.GetAuthenticatedAsync<List<FuncionarioCompetenciaViewModel>>($"api/FuncionarioCompetencias/funcionario/{id}");

        var horarios = await _apiService.GetAuthenticatedAsync<List<MeuHorarioViewModel>>( "api/HorarioFuncionarios");

        horarios ??= new List<MeuHorarioViewModel>();

        competencias ??= new List<CompetenciaViewModel>();
        competenciasFuncionario ??= new List<FuncionarioCompetenciaViewModel>();

        servicos ??= new List<ServicoViewModel>();
        associacoes ??= new List<FuncionarioServicoViewModel>();

        var model = new FuncionarioFormViewModel
        {
            Id = funcionario.Id,
            NomeCompleto = funcionario.NomeCompleto,
            Email = funcionario.Email ?? string.Empty,
            Telefone = funcionario.Telefone,
            Biografia = funcionario.Biografia,
            DataAdmissao = funcionario.DataAdmissao,
            Disponivel = funcionario.Disponivel,
            Ativo = funcionario.Ativo,
            UserId = funcionario.UserId,
            RoleAtual = "Funcionario",
            PermissaoAdminTemporariaAtiva = permissaoAtiva,

            Servicos = servicos
                .Where(s => s.Disponivel || associacoes.Any(a => a.ServicoId == s.Id))
                .OrderBy(s => s.CategoriaNome)
                .ThenBy(s => s.Nome)
                .Select(s =>
                {
                    var associacao = associacoes.FirstOrDefault(a => a.ServicoId == s.Id);

                    return new FuncionarioServicoSelecaoViewModel
                    {
                        ServicoId = s.Id,
                        Nome = s.Nome,
                        CategoriaNome = s.CategoriaNome,
                        Selecionado = associacao?.Ativo == true,
                        FuncionarioServicoId = associacao?.Id
                    };
                }).ToList(),

            CompetenciasDisponiveis = competencias
            .Where(c => c.Ativa)
            .OrderBy(c => c.Nome)
            .ToList(),

            CompetenciasFuncionario = competenciasFuncionario
            .OrderBy(c => c.CompetenciaNome)
            .ToList(),
            

            Horarios = horarios
            .Where(h => h.FuncionarioId == id)
            .OrderBy(h => h.DiaSemana == DayOfWeek.Sunday ? 7 : (int)h.DiaSemana)
            .ThenBy(h => h.HoraInicio)
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

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(model.NomeCompleto), "NomeCompleto");
        content.Add(new StringContent(model.Email), "Email");

        if (!string.IsNullOrWhiteSpace(model.Telefone))
        {
            content.Add(new StringContent(model.Telefone), "Telefone");
        }

        if (!string.IsNullOrWhiteSpace(model.Biografia))
        {
            content.Add(new StringContent(model.Biografia), "Biografia");
        }

        if (model.DataAdmissao.HasValue)
        {
            content.Add(new StringContent(model.DataAdmissao.Value.ToString("yyyy-MM-dd")), "DataAdmissao");
        }

        content.Add(new StringContent(model.Disponivel.ToString().ToLowerInvariant()), "Disponivel");
        content.Add(new StringContent(model.Ativo.ToString().ToLowerInvariant()), "Ativo");

        using var response = await _apiService.SendAuthenticatedMultipartAsync(HttpMethod.Put, $"api/Funcionarios/{model.Id}", content);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(erro)
                    ? "Não foi possível atualizar o funcionário."
                    : erro);

            return View(model);
        }

        var servicosAtualizados = await AtualizarServicosFuncionarioAsync(model.Id, model.Servicos);

        if (!servicosAtualizados)
        {
            ModelState.AddModelError(string.Empty, "Os dados do funcionário foram atualizados, mas ocorreu um erro ao atualizar os serviços.");

            return View(model);
        }

        TempData["SuccessMessage"] = "Funcionário e serviços atualizados com sucesso.";

        return RedirectToAction(nameof(Index));
    }


    private async Task<bool> AtualizarServicosFuncionarioAsync(int funcionarioId, IEnumerable<FuncionarioServicoSelecaoViewModel> servicos)
    {
        foreach (var servico in servicos)
        {
            // Nunca existiu associação e foi selecionado -> CRIAR
            if (servico.Selecionado && !servico.FuncionarioServicoId.HasValue)
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

            // A partir daqui só interessam associações já existentes
            if (!servico.FuncionarioServicoId.HasValue)
            {
                continue;
            }

            var associacao = await _apiService.GetAuthenticatedAsync<FuncionarioServicoViewModel>($"api/FuncionarioServicos/{servico.FuncionarioServicoId.Value}");

            if (associacao == null)
            {
                return false;
            }

            // Existia, estava inativa e voltou a ser selecionada -> REATIVAR
            if (servico.Selecionado && !associacao.Ativo)
            {
                var dto = new FuncionarioServicoRequestViewModel
                {
                    Id = associacao.Id,
                    FuncionarioId = associacao.FuncionarioId,
                    ServicoId = associacao.ServicoId,
                    PrecoPersonalizado = associacao.PrecoPersonalizado,
                    DuracaoPersonalizadaMinutos = associacao.DuracaoPersonalizadaMinutos,
                    Ativo = true
                };

                using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Put, $"api/FuncionarioServicos/{associacao.Id}", dto);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                continue;
            }

            // Existia, estava ativa e foi desmarcada -> DESATIVAR
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


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdicionarCompetencia([FromBody] FuncionarioCompetenciaRequestViewModel model)
    {
        if (model.FuncionarioId <= 0 || model.CompetenciaId <= 0)
        {
            return BadRequest(new
            {
                mensagem = "Selecione uma competência válida."
            });
        }

        using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Post, "api/FuncionarioCompetencias", model);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            return BadRequest(new
            {
                mensagem = string.IsNullOrWhiteSpace(erro)
                    ? "Não foi possível adicionar a competência."
                    : erro
            });
        }

        return Ok(new
        {
            sucesso = true
        });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverCompetencia(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                mensagem = "Competência inválida."
            });
        }

        using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Delete, $"api/FuncionarioCompetencias/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            return BadRequest(new
            {
                mensagem = string.IsNullOrWhiteSpace(erro)
                    ? "Não foi possível remover a competência."
                    : erro
            });
        }

        return Ok(new
        {
            sucesso = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarRole(string userId, string novaRole, int funcionarioId)
    {
        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(novaRole))
        {
            return BadRequest();
        }

        var model = new AlterarRoleViewModel
        {
            NovaRole = novaRole
        };

        using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Patch, $"api/Admin/users/{userId}/role", model);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] =
                string.IsNullOrWhiteSpace(erro)
                    ? "Não foi possível alterar o perfil do utilizador."
                    : erro;

            return RedirectToAction(nameof(Editar), new { id = funcionarioId });
        }

        TempData["SuccessMessage"] = $"Perfil alterado para {novaRole} com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConcederAdminTemporario(ConcederPermissaoTemporariaViewModel model, int funcionarioId)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Indique uma duração válida para o acesso temporário.";

            return RedirectToAction(nameof(Editar), new { id = funcionarioId });
        }

        using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Post, "api/Admin/permissoes-temporarias", model);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] =
                string.IsNullOrWhiteSpace(erro)
                    ? "Não foi possível conceder o acesso administrativo temporário."
                    : erro;

            return RedirectToAction(nameof(Editar), new { id = funcionarioId });
        }

        TempData["SuccessMessage"] = "Acesso administrativo temporário concedido com sucesso.";

        return RedirectToAction(nameof(Editar), new { id = funcionarioId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevogarAdminTemporario(int permissaoId, int funcionarioId)
    {
        using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Patch, $"api/Admin/permissoes-temporarias/{permissaoId}/revogar", new { });

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] =
                string.IsNullOrWhiteSpace(erro)
                    ? "Não foi possível revogar o acesso administrativo temporário."
                    : erro;

            return RedirectToAction(nameof(Editar), new { id = funcionarioId });
        }

        TempData["SuccessMessage"] = "Acesso administrativo temporário revogado com sucesso.";

        return RedirectToAction(nameof(Editar), new { id = funcionarioId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdicionarHorario( CriarHorarioFuncionarioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                sucesso = false,
                mensagem = "Preencha corretamente os dados do horário."
            });
        }

        using var response =  await _apiService.SendAuthenticatedJsonAsync(  HttpMethod.Post, "api/HorarioFuncionarios",  model);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            return BadRequest(new
            {
                sucesso = false,
                mensagem = string.IsNullOrWhiteSpace(erro)
                    ? "Não foi possível adicionar o horário."
                    : erro
            });
        }

        var horarioCriado = await response.Content.ReadFromJsonAsync<MeuHorarioViewModel>();

        if (horarioCriado == null)
        {
            return Ok(new
            {
                sucesso = true,
                mensagem = "Horário adicionado com sucesso."
            });
        }

        return Ok(new
        {
            sucesso = true,
            mensagem = "Horário adicionado com sucesso.",

            horario = new
            {
                id = horarioCriado.Id,
                funcionarioId = horarioCriado.FuncionarioId,
                diaSemana = horarioCriado.DiaSemana,
                horaInicio = horarioCriado.HoraInicio.ToString(@"hh\:mm"),
                horaFim = horarioCriado.HoraFim.ToString(@"hh\:mm")
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarHorario(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                sucesso = false,
                mensagem = "Horário inválido."
            });
        }

        using var response = await _apiService.SendAuthenticatedAsync( HttpMethod.Delete, $"api/HorarioFuncionarios/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            return BadRequest(new
            {
                sucesso = false,
                mensagem = string.IsNullOrWhiteSpace(erro)
                    ? "Não foi possível eliminar o horário."
                    : erro
            });
        }

        return Ok(new
        {
            sucesso = true,
            mensagem = "Horário eliminado com sucesso."
        });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarHorario(int id, int funcionarioId, DayOfWeek diaSemana, TimeSpan horaInicio, TimeSpan horaFim)
    {
        if (id <= 0 || funcionarioId <= 0)
        {
            return BadRequest(new
            {
                sucesso = false,
                mensagem = "Horário inválido."
            });
        }

        var model = new
        {
            Id = id,
            FuncionarioId = funcionarioId,
            DiaSemana = diaSemana,
            HoraInicio = horaInicio,
            HoraFim = horaFim
        };

        using var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Put, $"api/HorarioFuncionarios/{id}",model);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();

            return BadRequest(new
            {
                sucesso = false,
                mensagem = string.IsNullOrWhiteSpace(erro)
                    ? "Não foi possível atualizar o horário."
                    : erro
            });
        }

        return Ok(new
        {
            sucesso = true,
            mensagem = "Horário atualizado com sucesso.",
            horario = new
            {
                id,
                funcionarioId,
                diaSemana,
                horaInicio = horaInicio.ToString(@"hh\:mm"),
                horaFim = horaFim.ToString(@"hh\:mm")
            }
        });
    }

}