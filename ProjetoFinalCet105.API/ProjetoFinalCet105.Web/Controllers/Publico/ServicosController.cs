using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Publico
{
    public class ServicosController : Controller
    {
        private readonly ApiService _apiService;

        public ServicosController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var servicos = await _apiService.GetAsync<List<ServicoViewModel>>("api/Servicos");

            return View(servicos ?? new List<ServicoViewModel>());
        }
    }
}