namespace ProjetoFinalCet105.Web.Models;

public class FuncionarioCompetenciaRequestViewModel
{
    public int Id { get; set; }
    public int FuncionarioId { get; set; }
    public int CompetenciaId { get; set; }
    public string? Nivel { get; set; }
    public string? Certificacao { get; set; }
}
