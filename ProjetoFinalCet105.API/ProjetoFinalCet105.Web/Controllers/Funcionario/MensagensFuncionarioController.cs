using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Funcionario;

[Authorize(Roles = "Funcionario")]
public class MensagensFuncionarioController : Controller
{
    private readonly ApiService _apiService;

    public MensagensFuncionarioController( ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var conversas = await _apiService.GetAuthenticatedAsync<List<ConversaViewModel>>("api/Conversas")?? new List<ConversaViewModel>();

            conversas = conversas
                .OrderByDescending(c =>
                    c.Mensagens.Any()
                        ? c.Mensagens.Max(m => m.DataEnvio)
                        : c.DataCriacao)
                .ToList();

            return View(conversas);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível carregar as suas mensagens.";

            return View( new List<ConversaViewModel>());
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

            using var marcarLidasResponse = await _apiService.SendAuthenticatedAsync( HttpMethod.Put, $"api/Conversas/{id}/mensagens/lidas");

            return View(conversa);
        }
        catch (Exception)
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
            return RedirectToAction( nameof(Conversa),new { id });
        }

        try
        {
            var dados = new
            {
                Texto = texto.Trim()
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Post, $"api/Conversas/{id}/mensagens", dados);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Não foi possível enviar a mensagem.";
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível enviar a mensagem.";
        }

        return RedirectToAction( nameof(Conversa), new { id });
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
    public async Task<IActionResult> Nova()
    {
        try
        {
            var clientes = await _apiService.GetAuthenticatedAsync<List<ClienteViewModel>>( "api/Clientes") ?? new List<ClienteViewModel>();

            clientes = clientes
                .Where(c => c.Ativo)
                .OrderBy(c => c.NomeCompleto)
                .ToList();

            return View(clientes);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os clientes.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarConversa(string destinatarioId)
    {
        if (string.IsNullOrWhiteSpace(destinatarioId))
        {
            TempData["ErrorMessage"] =
                "Selecione um cliente para iniciar a conversa.";

            return RedirectToAction(nameof(Nova));
        }

        try
        {
            var dados = new
            {
                DestinatarioId = destinatarioId
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Post,"api/Conversas", dados);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] ="Não foi possível iniciar a conversa.";

                return RedirectToAction(nameof(Nova));
            }

            var conversa = await response.Content.ReadFromJsonAsync<ConversaViewModel>();

            if (conversa == null || conversa.Id <= 0)
            {
                TempData["ErrorMessage"] = "Não foi possível iniciar a conversa.";

                return RedirectToAction(nameof(Nova));
            }

            return RedirectToAction( nameof(Conversa), new { id = conversa.Id });
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Não foi possível iniciar a conversa.";

            return RedirectToAction(nameof(Nova));
        }
    }

    [HttpGet]
    public async Task<IActionResult> ContadorNaoLidas()
    {
        try
        {
            var contador = await _apiService.GetAuthenticatedAsync<int>( "api/Conversas/contador-nao-lidas");

            return Json(new
            {
                contador
            });
        }
        catch (Exception)
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
                return StatusCode(
                    (int)response.StatusCode
                );
            }

            return Ok();
        }
        catch (Exception)
        {
            return StatusCode(500);
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
            var conversa = await _apiService.GetAuthenticatedAsync<ConversaViewModel>( $"api/Conversas/{id}");

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
                    m.RemetenteId != conversa.FuncionarioUserId);

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
}
