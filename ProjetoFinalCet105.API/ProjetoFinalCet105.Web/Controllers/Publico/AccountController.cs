using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;
using System.Security.Claims;

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
        public IActionResult Login()
        {
            return View();
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

            var response = await _apiService.PostAsync<object, LoginResponseViewModel>("api/Auth/login", request);

            if (response == null)
            {
                ModelState.AddModelError(string.Empty, "Email ou password incorretos.");

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

            // Redirecionamento conforme o perfil
            //if (response.Roles.Contains("Admin"))
            //{
            //    return RedirectToAction( "Index", "Dashboard", new { area = "Admin" });
            //}

            //if (response.Roles.Contains("Funcionario"))
            //{
            //    return RedirectToAction("Index","Dashboard", new { area = "Funcionario" });
            //}

            //if (response.Roles.Contains("Cliente"))
            //{
            //    return RedirectToAction( "Index", "Dashboard", new { area = "Cliente" });
            //}


            return RedirectToAction("Index", "Home");
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
