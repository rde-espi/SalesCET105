namespace ProjetoFinalCet105.Web.Models;

public class HistoricoMarcacaoViewModel
{
    public int Id { get; set; }
    public int MarcacaoId { get; set; }

    public string UserId { get; set; } = string.Empty;
    public string UserNome { get; set; } = string.Empty;

    public string Acao { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public DateTime DataAlteracao { get; set; }
}
