namespace ProjetoFinalCet105.Web.Models;

public class GestaoFaturasViewModel
{
    public List<FaturaViewModel> Faturas { get; set; } = new();

    public int TotalFaturas { get; set; }
    public int Emitidas { get; set; }
    public int Anuladas { get; set; }

    public decimal TotalFaturado { get; set; }
}