namespace ProjetoFinalCet105.Web.Models;

public class FuncionarioServicoRequestViewModel
{
    public int Id { get; set; }
    public int FuncionarioId { get; set; }
    public int ServicoId { get; set; }
    public decimal? PrecoPersonalizado { get; set; }
    public int? DuracaoPersonalizadaMinutos { get; set; }
    public bool Ativo { get; set; }
}
