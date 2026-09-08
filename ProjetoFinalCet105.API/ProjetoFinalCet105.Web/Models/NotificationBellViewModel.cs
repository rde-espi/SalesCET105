namespace ProjetoFinalCet105.Web.Models
{
    public class NotificationBellViewModel
    {
        public int NaoLidas { get; set; }

        public List<NotificacaoDashboardViewModel> Notificacoes { get; set; } = new();
    }
}
