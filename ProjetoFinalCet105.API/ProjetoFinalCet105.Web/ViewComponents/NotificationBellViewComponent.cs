using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.ViewComponents
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly ApiService _apiService;

        public NotificationBellViewComponent(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var notificacoes =
                await _apiService.GetAuthenticatedAsync<List<NotificacaoDashboardViewModel>>(
                    "api/Notificacoes")
                ?? new();

            var contador =
                await _apiService.GetAuthenticatedAsync<int>(
                    "api/Notificacoes/contador-nao-lidas");

            var model = new NotificationBellViewModel
            {
                NaoLidas = contador,
                Notificacoes = notificacoes.Take(5).ToList()
            };

            return View(model);
        }
    }
}