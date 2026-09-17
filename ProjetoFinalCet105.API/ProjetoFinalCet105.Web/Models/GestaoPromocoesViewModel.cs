namespace ProjetoFinalCet105.Web.Models;

public class GestaoPromocoesViewModel
{
    public List<PromoCodeViewModel> Promocoes { get; set; } = new();

    public int Total { get; set; }
    public int Ativas { get; set; }
    public int Expiradas { get; set; }
    public int Utilizacoes { get; set; }
}