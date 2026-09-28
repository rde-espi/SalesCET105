using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Publico
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;

        public HomeController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var categorias = await _apiService.GetAsync<List<CategoriaViewModel>>("api/Categorias");

            return View(categorias ?? new List<CategoriaViewModel>());
        }

        [HttpGet]
        public IActionResult Sobre()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contactos()
        {
            return View(new ContactoViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contactos(ContactoViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var resultado = await _apiService.PostAsync<ContactoViewModel, ContactoRespostaViewModel>("api/Contactos", model);

                if (resultado == null)
                {
                    ModelState.AddModelError(string.Empty, "Não foi possível enviar a sua mensagem. Tente novamente.");

                    return View(model);
                }

                TempData["ContactoSuccessMessage"] = resultado.Mensagem;

                return RedirectToAction(nameof(Contactos));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Não foi possível enviar a sua mensagem. Tente novamente.");

                return View(model);
            }
        }
    }
}