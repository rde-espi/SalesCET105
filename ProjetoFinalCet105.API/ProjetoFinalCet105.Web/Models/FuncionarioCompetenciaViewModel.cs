namespace ProjetoFinalCet105.Web.Models;

public class FuncionarioCompetenciaViewModel
{
    public int Id { get; set; }
    public int FuncionarioId { get; set; }
    public string FuncionarioNome { get; set; } = string.Empty;
    public int CompetenciaId { get; set; }
    public string CompetenciaNome { get; set; } = string.Empty;
    public string? Nivel { get; set; }
    public string? Certificacao { get; set; }
}
