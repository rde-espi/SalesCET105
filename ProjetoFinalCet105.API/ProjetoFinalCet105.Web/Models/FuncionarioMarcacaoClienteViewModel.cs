namespace ProjetoFinalCet105.Web.Models;

public class FuncionarioMarcacaoClienteViewModel
{
    public int Id { get; set; }

    public string NomeCompleto { get; set; } = string.Empty;

    public string? Biografia { get; set; }

    public double MediaAvaliacao { get; set; }

    public int TotalAvaliacoes { get; set; }

    public string? FotografiaUrl { get; set; }
}