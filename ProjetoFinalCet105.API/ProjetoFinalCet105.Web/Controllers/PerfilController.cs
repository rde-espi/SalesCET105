using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers;

[Authorize]
public class PerfilController : Controller
{
    private readonly ApiService _apiService;

    public PerfilController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> Fotografia()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return NotFound();
        }

        HttpResponseMessage response;

        if (User.IsInRole("Funcionario"))
        {
            var funcionario = await _apiService.GetAuthenticatedAsync<FuncionarioViewModel>($"api/Funcionarios/user/{userId}");

            if (funcionario == null)
            {
                return NotFound();
            }

            response = await _apiService.SendAuthenticatedAsync(HttpMethod.Get, $"api/Funcionarios/{funcionario.Id}/fotografia");
        }
        else if (User.IsInRole("Cliente"))
        {
            response = await _apiService.SendAuthenticatedAsync(HttpMethod.Get, $"api/Clientes/{userId}/fotografia");
        }
        else
        {
            return NotFound();
        }

        using (response)
        {
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
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            if (User.IsInRole("Cliente"))
            {
                var cliente = await _apiService.GetAuthenticatedAsync<ClienteViewModel>($"api/Clientes/{userId}");

                if (cliente == null)
                {
                    return NotFound();
                }

                var googleCalendar = await _apiService.GetAuthenticatedAsync<GoogleCalendarStatusViewModel>("api/GoogleCalendar/status");

                var model = new PerfilViewModel
                {
                    UserId = cliente.Id,
                    TipoUtilizador = "Cliente",
                    NomeCompleto = cliente.NomeCompleto,
                    Email = cliente.Email,
                    Telefone = cliente.Telefone,
                    Contribuinte = cliente.Contribuinte,
                    Morada = cliente.Morada,
                    CodigoPostal = cliente.CodigoPostal,
                    Localidade = cliente.Localidade,
                    GoogleCalendarLigado = googleCalendar?.Ligado ?? false,
                    GoogleEmail = googleCalendar?.GoogleEmail,

                    FotografiaUrl = Url.Action("Fotografia", "Perfil")
                };

                return View(model);
            }

            if (User.IsInRole("Funcionario"))
            {
                var funcionario = await _apiService.GetAuthenticatedAsync<FuncionarioViewModel>($"api/Funcionarios/user/{userId}");


                if (funcionario == null)
                {
                    return NotFound();
                }
                var fotografiaResponse = await _apiService.SendAuthenticatedAsync(HttpMethod.Get, $"api/Funcionarios/{funcionario.Id}/fotografia");

                var temFotografia = fotografiaResponse.IsSuccessStatusCode;

                fotografiaResponse.Dispose();

                var googleCalendar = await _apiService.GetAuthenticatedAsync<GoogleCalendarStatusViewModel>("api/GoogleCalendar/status");

                var model = new PerfilViewModel
                {
                    UserId = funcionario.UserId,
                    TipoUtilizador = "Funcionário",
                    NomeCompleto = funcionario.NomeCompleto,
                    Email = funcionario.Email ?? string.Empty,
                    Telefone = funcionario.Telefone,
                    Biografia = funcionario.Biografia,
                    Disponivel = funcionario.Disponivel,
                    GoogleCalendarLigado = googleCalendar?.Ligado ?? false,
                    GoogleEmail = googleCalendar?.GoogleEmail,

                    TemFotografia = temFotografia,
                    FotografiaUrl = temFotografia ? Url.Action("FotografiaFuncionario", "Perfil") : null
                };

                return View(model);
            }

            if (User.IsInRole("Admin"))
            {
                var googleCalendar = await _apiService.GetAuthenticatedAsync<GoogleCalendarStatusViewModel>("api/GoogleCalendar/status");

                var model = new PerfilViewModel
                {
                    UserId = userId,
                    TipoUtilizador = "Administrador",
                    NomeCompleto =
                        User.FindFirst("NomeCompleto")?.Value
                        ?? User.Identity?.Name
                        ?? string.Empty,

                    Email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,

                    GoogleCalendarLigado = googleCalendar?.Ligado ?? false,
                    GoogleEmail = googleCalendar?.GoogleEmail
                };

                return View(model);
            }

            return Forbid();
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar o perfil.";

            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public async Task<IActionResult> FotografiaFuncionario()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return NotFound();
        }

        var funcionario = await _apiService.GetAuthenticatedAsync<FuncionarioViewModel>($"api/Funcionarios/user/{userId}");

        if (funcionario == null)
        {
            return NotFound();
        }

        using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Get, $"api/Funcionarios/{funcionario.Id}/fotografia");

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

        return File(bytes, contentType);
    }

    [HttpGet]
    public async Task<IActionResult> Editar()
    {
        if (!User.IsInRole("Funcionario"))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            var funcionario = await _apiService.GetAuthenticatedAsync<FuncionarioViewModel>($"api/Funcionarios/user/{userId}");

            if (funcionario == null)
            {
                return NotFound();
            }

            var model = new EditarPerfilFuncionarioViewModel
            {
                FuncionarioId = funcionario.Id,
                NomeCompleto = funcionario.NomeCompleto,
                Email = funcionario.Email ?? string.Empty,
                Telefone = funcionario.Telefone,
                Biografia = funcionario.Biografia,
                Disponivel = funcionario.Disponivel
            };

            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os dados do perfil.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarPerfilFuncionarioViewModel model)
    {
        if (!User.IsInRole("Funcionario"))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var funcionario = await _apiService.GetAuthenticatedAsync<FuncionarioViewModel>($"api/Funcionarios/user/{userId}");

            if (funcionario == null)
            {
                return NotFound();
            }

            // Segurança:
            // não usamos o FuncionarioId recebido do formulário
            // para decidir quem será alterado.
            model.FuncionarioId = funcionario.Id;

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

            content.Add(new StringContent(model.Disponivel.ToString()), "Disponivel");

            using var response = await _apiService.SendAuthenticatedMultipartAsync(HttpMethod.Put, $"api/Funcionarios/{funcionario.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(erro)
                        ? "Não foi possível atualizar o perfil."
                        : erro);

                return View(model);
            }

            TempData["SuccessMessage"] = "Perfil atualizado com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao atualizar o perfil.");

            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> AlterarFotografia()
    {
        if (!User.IsInRole("Funcionario"))
            return Forbid();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var funcionario = await _apiService.GetAuthenticatedAsync<FuncionarioViewModel>($"api/Funcionarios/user/{userId}");

            if (funcionario == null)
                return NotFound();

            var temFotografia = false;

            try
            {
                using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Get, $"api/Funcionarios/{funcionario.Id}/fotografia");

                temFotografia = response.IsSuccessStatusCode;
            }
            catch
            {
                temFotografia = false;
            }

            var model = new AlterarFotografiaPerfilViewModel
            {
                FuncionarioId = funcionario.Id,
                NomeCompleto = funcionario.NomeCompleto,
                TemFotografia = temFotografia,
                FotografiaUrl = temFotografia
                    ? Url.Action("Fotografia", "Perfil")
                    : null
            };

            return View(model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar a fotografia do perfil.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarFotografia(AlterarFotografiaPerfilViewModel model)
    {
        if (!User.IsInRole("Funcionario"))
            return Forbid();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (model.Fotografia == null || model.Fotografia.Length == 0)
        {
            ModelState.AddModelError(nameof(model.Fotografia), "Selecione uma fotografia.");

            return View(model);
        }

        try
        {
            var funcionario = await _apiService.GetAuthenticatedAsync<FuncionarioViewModel>($"api/Funcionarios/user/{userId}");

            if (funcionario == null)
                return NotFound();

            model.FuncionarioId = funcionario.Id;
            model.NomeCompleto = funcionario.NomeCompleto;

            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(funcionario.NomeCompleto), "NomeCompleto");

            content.Add(new StringContent(funcionario.Email ?? string.Empty), "Email");

            if (!string.IsNullOrWhiteSpace(funcionario.Telefone))
            {
                content.Add(new StringContent(funcionario.Telefone), "Telefone");
            }

            if (!string.IsNullOrWhiteSpace(funcionario.Biografia))
            {
                content.Add(new StringContent(funcionario.Biografia), "Biografia");
            }

            content.Add(new StringContent(funcionario.Disponivel.ToString()), "Disponivel");

            await using var stream = model.Fotografia.OpenReadStream();

            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.Fotografia.ContentType);

            content.Add(fileContent, "Fotografia", model.Fotografia.FileName);

            using var response = await _apiService.SendAuthenticatedMultipartAsync(HttpMethod.Put, $"api/Funcionarios/{funcionario.Id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(erro)
                        ? "Não foi possível atualizar a fotografia."
                        : erro);

                return View(model);
            }

            TempData["SuccessMessage"] = "Fotografia atualizada com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao atualizar a fotografia.");

            return View(model);
        }
    }

    [HttpGet]
    public IActionResult AlterarPassword()
    {
        return View(new AlterarPasswordViewModel());
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarPassword(AlterarPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var dados = new
            {
                PasswordAtual = model.PasswordAtual,
                NovaPassword = model.NovaPassword
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Post, "api/Auth/alterar-password", dados);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(erro)
                        ? "Não foi possível alterar a palavra-passe."
                        : erro);

                return View(model);
            }

            TempData["SuccessMessage"] = "Palavra-passe alterada com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao alterar a palavra-passe.");

            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> TwoFactor()
    {
        try
        {
            var estado = await _apiService.GetAuthenticatedAsync<TwoFactorViewModel>("api/Auth/2fa");

            if (estado == null)
            {
                TempData["ErrorMessage"] = "Não foi possível consultar o estado da autenticação de dois fatores.";

                return RedirectToAction(nameof(Index));
            }

            return View(estado);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível consultar o estado da autenticação de dois fatores.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TwoFactor(TwoFactorViewModel model)
    {
        try
        {
            var dados = new
            {
                Ativo = model.Ativo
            };

            using var response = await _apiService.SendAuthenticatedJsonAsync(HttpMethod.Put, "api/Auth/2fa", dados);

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Não foi possível alterar a autenticação de dois fatores.";

                return RedirectToAction(nameof(TwoFactor));
            }

            TempData["SuccessMessage"] = model.Ativo
                ? "Autenticação de dois fatores ativada com sucesso."
                : "Autenticação de dois fatores desativada com sucesso.";

            return RedirectToAction(nameof(TwoFactor));
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível alterar a autenticação de dois fatores.";

            return RedirectToAction(nameof(TwoFactor));
        }
    }

    [HttpGet]
    public async Task<IActionResult> LigarGoogleCalendar()
    {
        try
        {
            var resultado = await _apiService.GetAuthenticatedAsync<GoogleCalendarConectarViewModel>("api/GoogleCalendar/conectar");

            if (resultado == null || string.IsNullOrWhiteSpace(resultado.AuthorizationUrl))
            {
                TempData["ErrorMessage"] = "Não foi possível iniciar a ligação ao Google Calendar.";

                return RedirectToAction(nameof(Index));
            }

            return Redirect(resultado.AuthorizationUrl);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível iniciar a ligação ao Google Calendar.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesligarGoogleCalendar()
    {
        try
        {
            using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Delete, "api/GoogleCalendar/desligar");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Não foi possível desligar o Google Calendar.";

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Google Calendar desligado com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível desligar o Google Calendar.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditarCliente()
    {
        if (!User.IsInRole("Cliente"))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            var cliente = await _apiService.GetAuthenticatedAsync<ClienteViewModel>($"api/Clientes/{userId}");

            if (cliente == null)
            {
                return NotFound();
            }

            var model = new EditarPerfilClienteViewModel
            {
                ClienteId = cliente.Id,
                NomeCompleto = cliente.NomeCompleto,
                Email = cliente.Email,
                Telefone = cliente.Telefone,
                Contribuinte = cliente.Contribuinte,
                Morada = cliente.Morada,
                CodigoPostal = cliente.CodigoPostal,
                Localidade = cliente.Localidade
            };

            return View("EditarCliente", model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar os dados do perfil.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCliente(EditarPerfilClienteViewModel model)
    {
        if (!User.IsInRole("Cliente"))
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return View("EditarCliente", model);
        }

        try
        {
            var cliente = await _apiService.GetAuthenticatedAsync<ClienteViewModel>($"api/Clientes/{userId}");

            if (cliente == null)
            {
                return NotFound();
            }

            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(model.NomeCompleto), "NomeCompleto");

            content.Add(new StringContent(model.Email), "Email");

            if (!string.IsNullOrWhiteSpace(model.Telefone))
            {
                content.Add(new StringContent(model.Telefone), "Telefone");
            }

            if (!string.IsNullOrWhiteSpace(model.Contribuinte))
            {
                content.Add(new StringContent(model.Contribuinte), "Contribuinte");
            }

            if (!string.IsNullOrWhiteSpace(model.Morada))
            {
                content.Add(new StringContent(model.Morada), "Morada");
            }

            if (!string.IsNullOrWhiteSpace(model.CodigoPostal))
            {
                content.Add(new StringContent(model.CodigoPostal), "CodigoPostal");
            }

            if (!string.IsNullOrWhiteSpace(model.Localidade))
            {
                content.Add(new StringContent(model.Localidade), "Localidade");
            }

            using var response = await _apiService.SendAuthenticatedMultipartAsync(HttpMethod.Put, $"api/Clientes/{userId}", content);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(erro)
                        ? "Não foi possível atualizar o perfil."
                        : erro);

                return View("EditarCliente", model);
            }

            TempData["SuccessMessage"] = "Perfil atualizado com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao atualizar o perfil.");

            return View("EditarCliente", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> AlterarFotografiaCliente()
    {
        if (!User.IsInRole("Cliente"))
            return Forbid();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            var cliente = await _apiService.GetAuthenticatedAsync<ClienteViewModel>($"api/Clientes/{userId}");

            if (cliente == null)
                return NotFound();

            var temFotografia = false;

            try
            {
                using var response = await _apiService.SendAuthenticatedAsync(HttpMethod.Get, $"api/Clientes/{userId}/fotografia");

                temFotografia = response.IsSuccessStatusCode;
            }
            catch
            {
                temFotografia = false;
            }

            var model = new AlterarFotografiaPerfilViewModel
            {
                NomeCompleto = cliente.NomeCompleto,
                TemFotografia = temFotografia,
                FotografiaUrl = temFotografia
                    ? Url.Action("Fotografia", "Perfil")
                    : null
            };

            return View("AlterarFotografiaCliente", model);
        }
        catch
        {
            TempData["ErrorMessage"] = "Não foi possível carregar a fotografia do perfil.";

            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarFotografiaCliente(AlterarFotografiaPerfilViewModel model)
    {
        if (!User.IsInRole("Cliente"))
            return Forbid();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (model.Fotografia == null || model.Fotografia.Length == 0)
        {
            ModelState.AddModelError(nameof(model.Fotografia), "Selecione uma fotografia.");

            return View("AlterarFotografiaCliente", model);
        }

        try
        {
            var cliente = await _apiService.GetAuthenticatedAsync<ClienteViewModel>($"api/Clientes/{userId}");

            if (cliente == null)
                return NotFound();

            model.NomeCompleto = cliente.NomeCompleto;
            model.FotografiaUrl = Url.Action("Fotografia", "Perfil");

            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(cliente.NomeCompleto), "NomeCompleto");

            content.Add(new StringContent(cliente.Email ?? string.Empty), "Email");

            if (!string.IsNullOrWhiteSpace(cliente.Telefone))
            {
                content.Add(new StringContent(cliente.Telefone), "Telefone");
            }

            if (!string.IsNullOrWhiteSpace(cliente.Contribuinte))
            {
                content.Add(new StringContent(cliente.Contribuinte), "Contribuinte");
            }

            if (!string.IsNullOrWhiteSpace(cliente.Morada))
            {
                content.Add(new StringContent(cliente.Morada), "Morada");
            }

            if (!string.IsNullOrWhiteSpace(cliente.CodigoPostal))
            {
                content.Add(new StringContent(cliente.CodigoPostal), "CodigoPostal");
            }

            if (!string.IsNullOrWhiteSpace(cliente.Localidade))
            {
                content.Add(new StringContent(cliente.Localidade), "Localidade");
            }

            await using var stream = model.Fotografia.OpenReadStream();

            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(model.Fotografia.ContentType);

            content.Add(fileContent, "Fotografia", model.Fotografia.FileName);

            using var response = await _apiService.SendAuthenticatedMultipartAsync(HttpMethod.Put, $"api/Clientes/{userId}", content);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(erro)
                        ? "Não foi possível atualizar a fotografia."
                        : erro);

                return View("AlterarFotografiaCliente", model);
            }

            TempData["SuccessMessage"] = "Fotografia atualizada com sucesso.";

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao atualizar a fotografia.");

            return View("AlterarFotografiaCliente", model);
        }
    }

}
