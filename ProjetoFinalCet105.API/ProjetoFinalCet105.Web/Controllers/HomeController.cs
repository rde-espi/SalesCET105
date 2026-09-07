using Microsoft.AspNetCore.Mvc;
using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;
using System.Diagnostics;

namespace ProjetoFinalCet105.Web.Controllers
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
            var categorias =
                await _apiService.GetAsync<List<CategoriaViewModel>>("api/Categorias");

            return View(categorias ?? new List<CategoriaViewModel>());
        }
    }
}
