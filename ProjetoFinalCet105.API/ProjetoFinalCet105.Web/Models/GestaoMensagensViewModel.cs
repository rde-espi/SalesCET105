namespace ProjetoFinalCet105.Web.Models;

public class GestaoMensagensViewModel
{
    public List<ConversaViewModel> Conversas { get; set; } = new List<ConversaViewModel>();

    public int TotalConversas { get; set; }
    public int TotalClientes { get; set; }
    public int TotalProfissionais { get; set; }
}
