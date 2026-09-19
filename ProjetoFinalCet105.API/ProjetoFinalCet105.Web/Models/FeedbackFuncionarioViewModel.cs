namespace ProjetoFinalCet105.Web.Models;

public class FeedbackFuncionarioViewModel
{
    public FeedbackResumoViewModel Resumo { get; set; } = new FeedbackResumoViewModel();

    public List<FeedbackViewModel> Feedbacks { get; set; } = new List<FeedbackViewModel>();
}
