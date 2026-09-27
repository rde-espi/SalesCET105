using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Publico
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Registar()
        {
            return View(new RegistarClienteViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registar(RegistarClienteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var content = new MultipartFormDataContent();

            content.Add( new StringContent(model.NomeCompleto), "NomeCompleto");

            content.Add( new StringContent(model.Email), "Email");

            content.Add( new StringContent(model.Password), "Password");

            if (!string.IsNullOrWhiteSpace(model.Telefone))
            {
                content.Add( new StringContent(model.Telefone), "Telefone");
            }

            if (!string.IsNullOrWhiteSpace(model.Contribuinte))
            {
                content.Add( new StringContent(model.Contribuinte), "Contribuinte");
            }

            if (!string.IsNullOrWhiteSpace(model.Morada))
            {
                content.Add( new StringContent(model.Morada), "Morada");
            }

            if (!string.IsNullOrWhiteSpace(model.CodigoPostal))
            {
                content.Add( new StringContent(model.CodigoPostal), "CodigoPostal");
            }

            if (!string.IsNullOrWhiteSpace(model.Localidade))
            {
                content.Add( new StringContent(model.Localidade), "Localidade");
            }

            if (model.Fotografia != null &&
                model.Fotografia.Length > 0)
            {
                var streamContent = new StreamContent(model.Fotografia.OpenReadStream());

                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( model.Fotografia.ContentType);

                content.Add( streamContent, "Fotografia", model.Fotografia.FileName);
            }

            var response = await _apiService.SendAuthenticatedMultipartAsync( HttpMethod.Post, "api/Clientes", content);

            if (!response.IsSuccessStatusCode)
            {
                var erroApi = await response.Content.ReadAsStringAsync();

                erroApi = erroApi.Trim().Trim('"');

                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(erroApi)
                        ? "Não foi possível criar a conta. Verifique os dados introduzidos."
                        : erroApi);

                return View(model);
            }

            return RedirectToAction( nameof(ConfirmarEmail), new { email = model.Email });
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmarEmail(string email, string? token = null)
        {
            
            if (string.IsNullOrWhiteSpace(token))
            {
                return View(new ConfirmarEmailViewModel
                {
                    Email = email
                });
            }
                        
            var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Post, "api/Auth/confirmar-email",
                new
                {
                    email,
                    token
                });

            if (!response.IsSuccessStatusCode)
            {
                return View(new ConfirmarEmailViewModel
                {
                    Email = email,
                    Codigo = string.Empty
                });
            }

            TempData["SuccessMessage"] = "Conta confirmada com sucesso. Já pode iniciar sessão.";

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEmail(ConfirmarEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Post, "api/Auth/confirmar-email",
                new
                {
                    email = model.Email,
                    token = model.Codigo
                });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError( string.Empty, "O código de confirmação é inválido ou expirou.");

                return View(model);
            }

            TempData["SuccessMessage"] =  "Conta confirmada com sucesso. Já pode iniciar sessão.";

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReenviarCodigoConfirmacao(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction(nameof(Login));
            }

            var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Post, "api/Auth/reenviar-confirmacao-email",
                new
                {
                    email
                });

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Não foi possível reenviar o código de confirmação.";

                return RedirectToAction( nameof(ConfirmarEmail), new { email });
            }

            TempData["SuccessMessage"] = "Enviámos um novo código de confirmação para o seu email.";

            return RedirectToAction(nameof(ConfirmarEmail), new { email });
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult TwoFactor()
        {
            var userId = HttpContext.Session.GetString("TwoFactorUserId");

            if (string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new VerificarTwoFactorViewModel
            {
                UserId = userId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TwoFactor(VerificarTwoFactorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = HttpContext.Session.GetString("TwoFactorUserId");

            if (string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var request = new
            {
                UserId = userId,
                Codigo = model.Codigo
            };

            var response = await _apiService.PostAsync<object, LoginResponseViewModel>("api/Auth/verificar-2fa", request);

            if (response == null || string.IsNullOrWhiteSpace(response.Token))
            {
                ModelState.AddModelError(string.Empty, "Código de autenticação inválido.");

                return View(model);
            }

            HttpContext.Session.SetString("JwtToken", response.Token);
            HttpContext.Session.SetString("UserId", response.UserId);
            HttpContext.Session.SetString("NomeCompleto", response.NomeCompleto);
            HttpContext.Session.SetString("Email", response.Email);
            HttpContext.Session.SetString("Roles", string.Join(",", response.Roles));

            HttpContext.Session.Remove("TwoFactorUserId");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, response.UserId),
                new Claim(ClaimTypes.Name, response.NomeCompleto),
                new Claim(ClaimTypes.Email, response.Email)
            };

            foreach (var role in response.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            if (response.Roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (response.Roles.Contains("Funcionario"))
            {
                return RedirectToAction("Index", "Funcionarios");
            }

            if (response.Roles.Contains("Cliente"))
            {
                return RedirectToAction("Index", "DashboardCliente");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RecuperarPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarPassword( RecuperarPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Post, "api/Auth/recuperar-password",
                new
                {
                    email = model.Email
                });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError( string.Empty, "Não foi possível processar o pedido. Tente novamente.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Se existir uma conta associada a este email, receberá instruções para recuperar a palavra-passe.";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword( string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword( ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Post, "api/Auth/reset-password",
                new
                {
                    email = model.Email,
                    token = model.Token,
                    novaPassword = model.NovaPassword
                });

            if (!response.IsSuccessStatusCode)
            {
                var erroApi = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError(
                    string.Empty,
                    string.IsNullOrWhiteSpace(erroApi)
                        ? "Não foi possível alterar a palavra-passe."
                        : erroApi.Trim().Trim('"'));

                return View(model);
            }

            TempData["SuccessMessage"] = "Palavra-passe alterada com sucesso. Já pode iniciar sessão.";

            return RedirectToAction(nameof(Login));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new
            {
                Email = model.Email,
                Password = model.Password
            };

            var apiResponse = await _apiService.SendAuthenticatedJsonAsync( HttpMethod.Post, "api/Auth/login", request);
            if (!apiResponse.IsSuccessStatusCode)
            {
                var erroApi = await apiResponse.Content.ReadAsStringAsync();

                if (erroApi.Contains( "Confirme o seu email", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError( string.Empty, "Confirme o seu email antes de iniciar sessão.");

                    ViewBag.EmailNaoConfirmado = true;
                    ViewBag.EmailConfirmacao = model.Email;
                }
                else
                {
                    ModelState.AddModelError(string.Empty,"Email ou password incorretos.");
                }

                return View(model);
            }

            var response = await apiResponse.Content.ReadFromJsonAsync<LoginResponseViewModel>();

            if (response == null)
            {
                ModelState.AddModelError( string.Empty, "Não foi possível concluir o login.");

                return View(model);
            }


            // Caso a API exija 2FA
            if (response.RequiresTwoFactor)
            {
                HttpContext.Session.SetString("TwoFactorUserId", response.UserId);

                return RedirectToAction("TwoFactor");
            }


            if (string.IsNullOrWhiteSpace(response.Token))
            {
                ModelState.AddModelError(string.Empty, "Não foi possível concluir o login.");

                return View(model);
            }


            // Guardar dados da sessão
            HttpContext.Session.SetString("JwtToken", response.Token);
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    response.UserId),

                new Claim(
                    ClaimTypes.Name,
                    response.NomeCompleto),

                new Claim(
                    ClaimTypes.Email,
                    response.Email)
            };

            HttpContext.Session.SetString("UserId", response.UserId);

            HttpContext.Session.SetString("NomeCompleto", response.NomeCompleto);

            HttpContext.Session.SetString("Email", response.Email);

            HttpContext.Session.SetString("Roles", string.Join(",", response.Roles));

            foreach (var role in response.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            //Redirecionamento conforme o perfil
            if (response.Roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (response.Roles.Contains("Funcionario"))
            {
                return RedirectToAction("Index", "DashboardFuncionario");
            }

            if (response.Roles.Contains("Cliente"))
            {
                return RedirectToAction("Index", "DashboardCliente");
            }


            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GoogleLogin(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                TempData["ErrorMessage"] = "Não foi possível obter os dados da conta Google.";

                return RedirectToAction(nameof(Login));
            }

            var request = new
            {
                IdToken = idToken
            };

            var response = await _apiService.PostAsync<object, LoginResponseViewModel>("api/Auth/google", request);

            if (response == null)
            {
                TempData["ErrorMessage"] = "Não foi possível iniciar sessão com a conta Google.";

                return RedirectToAction(nameof(Login));
            }

            if (response.RequiresTwoFactor)
            {
                HttpContext.Session.SetString("TwoFactorUserId", response.UserId);

                return RedirectToAction(nameof(TwoFactor));
            }

            if (string.IsNullOrWhiteSpace(response.Token))
            {
                TempData["ErrorMessage"] = "Não foi possível concluir o login com Google.";

                return RedirectToAction(nameof(Login));
            }

            HttpContext.Session.SetString("JwtToken", response.Token);

            HttpContext.Session.SetString("UserId", response.UserId);

            HttpContext.Session.SetString("NomeCompleto", response.NomeCompleto);

            HttpContext.Session.SetString("Email", response.Email);

            HttpContext.Session.SetString("Roles", string.Join(",", response.Roles));

            var claims = new List<Claim>
    {
        new Claim( ClaimTypes.NameIdentifier, response.UserId),

        new Claim(ClaimTypes.Name,response.NomeCompleto),

        new Claim( ClaimTypes.Email, response.Email)
    };

            foreach (var role in response.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            if (response.Roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (response.Roles.Contains("Funcionario"))
            {
                return RedirectToAction("Index", "Funcionarios");
            }

            if (response.Roles.Contains("Cliente"))
            {
                return RedirectToAction("Index", "DashboardCliente");
            }

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult PrimeiroAcesso(string email, string tokenConfirmacao, string tokenPassword)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(tokenConfirmacao) || string.IsNullOrWhiteSpace(tokenPassword))
            {
                return RedirectToAction("Login");
            }

            var model = new PrimeiroAcessoFuncionarioViewModel
            {
                Email = email,
                TokenConfirmacao = tokenConfirmacao,
                TokenPassword = tokenPassword
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrimeiroAcesso(PrimeiroAcessoFuncionarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var confirmarEmailRequest = new
            {
                Email = model.Email,
                Token = model.TokenConfirmacao
            };

            var confirmacao = await _apiService.PostAsync<object, object>("api/Auth/confirmar-email", confirmarEmailRequest);

            if (confirmacao == null)
            {
                ModelState.AddModelError(string.Empty, "Não foi possível confirmar o email. O link poderá ser inválido ou ter expirado.");
                return View(model);
            }

            var definirPasswordRequest = new
            {
                Email = model.Email,
                Token = model.TokenPassword,
                NovaPassword = model.NovaPassword
            };

            var password = await _apiService.PostAsync<object, object>("api/Auth/reset-password", definirPasswordRequest);

            if (password == null)
            {
                ModelState.AddModelError(string.Empty, "O email foi confirmado, mas não foi possível definir a palavra-passe.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Conta ativada com sucesso. Já pode iniciar sessão.";

            return RedirectToAction("Login");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}