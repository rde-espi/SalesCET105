namespace ProjetoFinalCet105.Web.Models;

public class ConversaViewModel
{
    public int Id { get; set; }

    public string ClienteId { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;

    public string FuncionarioUserId { get; set; } = string.Empty;
    public string FuncionarioNome { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; }

    public List<MensagemViewModel> Mensagens { get; set; }
        = new List<MensagemViewModel>();
}