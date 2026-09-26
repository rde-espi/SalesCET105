using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;

using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Cliente;

[Authorize(Roles = "Cliente")]
public class MensagensClienteController : Controller
{
    private readonly ApiService _apiService;

    public MensagensClienteController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var conversas = await _apiService.GetAuthenticatedAsync<List<ConversaViewModel>>("api/Conversas") ?? new List<ConversaViewModel>();

            conversas = conversas
                .OrderByDescending(c =>
                    c.Mensagens.Any()
                        ? c.Mensagens.Max(m => m.DataEnvio)
                        : c.DataCriacao)
                .ToList();

            return View(conversas);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar as suas mensagens.";

            return View(new List<ConversaViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Conversa(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var conversa = await _apiService.GetAuthenticatedAsync<ConversaViewModel>($"api/Conversas/{id}");

            if (conversa == null)
            {
                TempData["ErrorMessage"] = "Conversa não encontrada.";

                return RedirectToAction(nameof(Index));
            }

            using var marcarLidasResponse = await _apiService.SendAuthenticatedAsync(HttpMethod.Put, $"api/Conversas/{id}/mensagens/lidas");

            return View(conversa);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar a conversa.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enviar(int id, string texto)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        if (string.IsNullOrWhiteSpace(texto))
        {
            return RedirectToAction(nameof(Conversa), new { id });
        }

        try
        {
            var dados = new
            {
                Texto = texto.Trim()
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Post, $"api/Conversas/{id}/mensagens", dados);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Não foi possível enviar a mensagem.";
            }
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível enviar a mensagem.";
        }

        return RedirectToAction(nameof(Conversa), new { id });
    }

    [HttpGet]
    public IActionResult SignalRToken()
    {
        var token = HttpContext.Session.GetString("JwtToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        return Json(new
        {
            accessToken = token
        });
    }

    [HttpGet]
    public async Task<IActionResult> ContadorNaoLidas()
    {
        try
        {
            var contador = await _apiService.GetAuthenticatedAsync<int>("api/Conversas/contador-nao-lidas");

            return Json(new
            {
                contador
            });
        }
        catch
        {
            return Json(new
            {
                contador = 0
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarcarComoLidas(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Put, $"api/Conversas/{id}/mensagens/lidas");

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            return Ok();
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ContadorNaoLidasConversa(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var conversa =
                await _apiService.GetAuthenticatedAsync<ConversaViewModel>(
                    $"api/Conversas/{id}");

            if (conversa == null)
            {
                return Json(new
                {
                    conversaId = id,
                    contador = 0
                });
            }

            var contador =
                conversa.Mensagens.Count(m =>
                    !m.Lida &&
                    m.RemetenteId != conversa.ClienteId);

            return Json(new
            {
                conversaId = id,
                contador
            });
        }
        catch (Exception)
        {
            return Json(new
            {
                conversaId = id,
                contador = 0
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Nova()
    {
        try
        {
            var funcionarios = await _apiService.GetAuthenticatedAsync<List<FuncionarioViewModel>>("api/Funcionarios") ?? new List<FuncionarioViewModel>();

            funcionarios = funcionarios
                .Where(f => f.Ativo && f.Disponivel)
                .OrderBy(f => f.NomeCompleto)
                .ToList();

            return View(funcionarios);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os profissionais.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarConversa(string destinatarioId)
    {
        if (string.IsNullOrWhiteSpace(destinatarioId))
        {
            TempData["ErrorMessage"] = "Selecione um profissional para iniciar a conversa.";

            return RedirectToAction(nameof(Nova));
        }

        try
        {
            var dados = new
            {
                DestinatarioId = destinatarioId
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Post, "api/Conversas", dados);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();

                TempData["ErrorMessage"] = $"Não foi possível iniciar a conversa. API: {(int)response.StatusCode} - {erro}";

                return RedirectToAction(nameof(Nova));
            }

            var conversa = await response.Content.ReadFromJsonAsync<ConversaViewModel>();
            if (conversa == null || conversa.Id <= 0)
            {
                TempData["ErrorMessage"] = "Não foi possível iniciar a conversa.";

                return RedirectToAction(nameof(Nova));
            }

            return RedirectToAction(nameof(Conversa), new { id = conversa.Id });
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível iniciar a conversa.";

            return RedirectToAction(nameof(Nova));
        }
    }
}
