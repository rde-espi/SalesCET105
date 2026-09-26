using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Cliente;

[Authorize(Roles = "Cliente")]
public class MarcacoesClienteController : Controller
{
    private readonly ApiService _apiService;

    public MarcacoesClienteController(ApiService apiService)
    {
        _apiService = apiService;
    }


    // =========================
    // MINHAS MARCAÇÕES
    // =========================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var marcacoes = await _apiService.GetAuthenticatedAsync<List<MarcacaoClienteViewModel>>("api/Marcacoes")?? new List<MarcacaoClienteViewModel>();

            marcacoes = marcacoes
                .OrderByDescending(m => m.DataHoraInicio)
                .ToList();

            var marcacoesAvaliadas = new HashSet<int>();

            var marcacoesConcluidas = marcacoes
                .Where(m =>
                {
                    var estado = (m.EstadoMarcacaoNome ?? "")
                        .Trim()
                        .ToLowerInvariant();

                    return estado == "concluida" ||
                           estado == "concluída";
                })
                .ToList();

            foreach (var marcacao in marcacoesConcluidas)
            {
                try
                {
                    var feedback = await _apiService.GetAuthenticatedAsync<FeedbackViewModel>($"api/Feedbacks/marcacao/{marcacao.Id}");

                    if (feedback != null)
                    {
                        marcacoesAvaliadas.Add(marcacao.Id);
                    }
                }
                catch (HttpRequestException ex)
                    when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Esta marcação ainda não foi avaliada.
                }
            }

            ViewBag.MarcacoesAvaliadas = marcacoesAvaliadas;

            return View(marcacoes);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar as suas marcações.";

            return View( new List<MarcacaoClienteViewModel>());
        }
    }


    // =========================
    // DETALHES
    // =========================

    [HttpGet]
    public async Task<IActionResult> Detalhes(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var marcacao = await _apiService.GetAuthenticatedAsync<MarcacaoClienteViewModel>($"api/Marcacoes/{id}");

            if (marcacao == null)
            {
                return NotFound();
            }

            FeedbackViewModel? feedback = null;

            try
            {
                feedback = await _apiService.GetAuthenticatedAsync<FeedbackViewModel>( $"api/Feedbacks/marcacao/{marcacao.Id}");
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // 404 significa que esta marcação ainda não foi avaliada.
            }

            ViewBag.Feedback = feedback;

            return View(marcacao);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os detalhes da marcação.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Nova(int? servicoId = null, int? funcionarioId = null)
    {
        try
        {
            var model = new NovaMarcacaoClienteViewModel
            {
                ServicoId = servicoId ?? 0,
                FuncionarioId = funcionarioId ?? 0
            };

            await CarregarServicos(model);

            ViewBag.ServicoPreSelecionadoId = servicoId;
            ViewBag.FuncionarioPreSelecionadoId = funcionarioId;

            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível preparar a nova marcação.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> FuncionariosPorServico(int servicoId)
    {
        if (servicoId <= 0)
        {
            return Json(new List<FuncionarioMarcacaoClienteViewModel>());
        }

        try
        {
            var funcionarios = await _apiService.GetAuthenticatedAsync<List<FuncionarioViewModel>>($"api/Funcionarios/servico/{servicoId}") ?? new List<FuncionarioViewModel>();

            var resultado = new List<FuncionarioMarcacaoClienteViewModel>();

            foreach (var funcionario in funcionarios.OrderBy(f => f.NomeCompleto))
            {
                FeedbackResumoViewModel? resumo = null;

                try
                {
                    resumo = await _apiService.GetAuthenticatedAsync<FeedbackResumoViewModel>($"api/Feedbacks/funcionario/{funcionario.Id}/resumo");
                }
                catch
                {
                    // Um erro no rating não deve impedir o profissional de aparecer na marcação.
                }

                resultado.Add(new FuncionarioMarcacaoClienteViewModel
                {
                    Id = funcionario.Id,
                    NomeCompleto = funcionario.NomeCompleto,
                    Biografia = funcionario.Biografia,

                    MediaAvaliacao = resumo?.Media ?? 0,
                    TotalAvaliacoes = resumo?.TotalAvaliacoes ?? 0,

                    FotografiaUrl = Url.Action(nameof(FotografiaFuncionario), "MarcacoesCliente", new { id = funcionario.Id })
                });
            }

            return Json(resultado);
        }
        catch
        {
            return Json(new List<FuncionarioMarcacaoClienteViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> DetalhesServicoProfissional(int funcionarioId, int servicoId)
    {
        if (funcionarioId <= 0 || servicoId <= 0)
        {
            return BadRequest();
        }

        try
        {
            var servicos = await _apiService.GetAuthenticatedAsync<List<FuncionarioServicoViewModel>>($"api/FuncionarioServicos/funcionario/{funcionarioId}");

            var associacao = servicos?
                .FirstOrDefault(fs =>
                    fs.ServicoId == servicoId &&
                    fs.Ativo);

            if (associacao == null)
            {
                return NotFound();
            }

            return Json(new
            {
                precoPersonalizado = associacao.PrecoPersonalizado,

                duracaoPersonalizadaMinutos = associacao.DuracaoPersonalizadaMinutos
            });
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    public async Task<IActionResult> HorariosDisponiveis(int funcionarioId, int servicoId, DateTime data)
    {
        if (funcionarioId <= 0 || servicoId <= 0 || data == default)
        {
            return Json(new List<DateTime>());
        }

        try
        {
            var horarios = await _apiService.GetAuthenticatedAsync<List<DateTime>>(
                $"api/Marcacoes/disponibilidade" +
                $"?funcionarioId={funcionarioId}" +
                $"&servicoId={servicoId}" +
                $"&data={data:yyyy-MM-dd}");

            return Json(horarios ?? new List<DateTime>());
        }
        catch
        {
            return Json(new List<DateTime>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValidarPromoCode(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return BadRequest("Introduza um código promocional.");
        }

        try
        {
            var dto = new
            {
                Codigo = codigo.Trim()
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Post, "api/PromoCodes/validar", dto);

            var conteudo = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, conteudo);
            }

            return Content(conteudo, "application/json");
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Não foi possível validar o código promocional.");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nova(NovaMarcacaoClienteViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CarregarServicos(model);
            return View(model);
        }

        try
        {
            var dto = new
            {
                model.FuncionarioId,
                model.ServicoId,
                model.DataHoraInicio,
                model.Observacoes,
                model.PromoCode
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Post, "api/Marcacoes", dto);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(erro)
                        ? "Não foi possível criar a marcação."
                        : erro.Trim('"'));

                await CarregarServicos(model);

                return View(model);
            }

            TempData["SuccessMessage"] = "Marcação criada com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao criar a marcação.");

            await CarregarServicos(model);

            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> FotografiaFuncionario(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

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
    public async Task<IActionResult> DatasDisponiveis(int funcionarioId, int servicoId)
    {
        if (funcionarioId <= 0 || servicoId <= 0)
        {
            return Json(new List<object>());
        }

        try
        {
            var datasDisponiveis = new List<object>();

            var dataInicial = DateTime.Today;
            var dataFinal = dataInicial.AddDays(30);

            for (var data = dataInicial;
                 data <= dataFinal;
                 data = data.AddDays(1))
            {
                try
                {
                    var horarios = await _apiService.GetAuthenticatedAsync<List<DateTime>>(
                        $"api/Marcacoes/disponibilidade" +
                        $"?funcionarioId={funcionarioId}" +
                        $"&servicoId={servicoId}" +
                        $"&data={data:yyyy-MM-dd}");

                    if (horarios != null && horarios.Count > 0)
                    {
                        datasDisponiveis.Add(new
                        {
                            data = data.ToString("yyyy-MM-dd"),
                            diaSemana = data.ToString("ddd",
                                new System.Globalization.CultureInfo("pt-PT")),
                            dia = data.Day,
                            mes = data.ToString("MMM",
                                new System.Globalization.CultureInfo("pt-PT"))
                        });
                    }
                }
                catch
                {
                    // Um dia sem disponibilidade não impede a análise dos restantes dias.
                }
            }

            return Json(datasDisponiveis);
        }
        catch
        {
            return Json(new List<object>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> ImagemCategoria(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Get, $"api/Categorias/{id}/imagem");

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
    public async Task<IActionResult> ImagemServico(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Get, $"api/Servicos/{id}/imagem");

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










    private async Task CarregarServicos(NovaMarcacaoClienteViewModel model)
    {
        var servicos = await _apiService.GetAuthenticatedAsync<List<ServicoViewModel>>("api/Servicos");

        var categorias = await _apiService.GetAuthenticatedAsync<List<CategoriaViewModel>>("api/Categorias");

        model.Servicos = servicos?
            .Where(s => s.Disponivel)
            .OrderBy(s => s.Nome)
            .ToList()
            ?? new List<ServicoViewModel>();

        model.Categorias = categorias?
            .OrderBy(c => c.Nome)
            .ToList()
            ?? new List<CategoriaViewModel>();
    }

}