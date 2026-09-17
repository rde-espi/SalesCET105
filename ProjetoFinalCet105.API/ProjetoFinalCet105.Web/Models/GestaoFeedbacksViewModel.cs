namespace ProjetoFinalCet105.Web.Models;

public class GestaoFeedbacksViewModel
{
    public List<FeedbackViewModel> Feedbacks { get; set; } = new List<FeedbackViewModel>();

    public int TotalFeedbacks { get; set; }

    public double MediaGeral { get; set; }

    public int CincoEstrelas { get; set; }

    public int ProfissionaisAvaliados { get; set; }
}