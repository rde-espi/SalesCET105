namespace ProjetoFinalCet105.Web.Models;

public class FuncionarioServicoSelecaoViewModel
{
    public int ServicoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? CategoriaNome { get; set; }
    public bool Selecionado { get; set; }
    public int? FuncionarioServicoId { get; set; }
}