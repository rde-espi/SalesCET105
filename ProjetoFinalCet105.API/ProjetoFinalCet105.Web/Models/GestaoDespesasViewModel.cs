namespace ProjetoFinalCet105.Web.Models;

public class GestaoDespesasViewModel
{
    public List<DespesaViewModel> Despesas { get; set; } = new();

    public int TotalRegistos { get; set; }

    public decimal TotalDespesas { get; set; }

    public decimal TotalMes { get; set; }

    public decimal TotalAno { get; set; }
}