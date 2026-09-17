namespace ProjetoFinalCet105.Web.Models;

public class FeedbackViewModel
{
    public int Id { get; set; }
    public int MarcacaoId { get; set; }

    public string ClienteId { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;

    public int FuncionarioId { get; set; }
    public string FuncionarioNome { get; set; } = string.Empty;

    public int Classificacao { get; set; }
    public string? Comentario { get; set; }

    public DateTime DataCriacao { get; set; }
}
