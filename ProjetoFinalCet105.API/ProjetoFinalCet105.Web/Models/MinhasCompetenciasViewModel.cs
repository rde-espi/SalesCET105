namespace ProjetoFinalCet105.Web.Models;

public class MinhasCompetenciasViewModel
{
    public List<CompetenciaViewModel> CompetenciasDisponiveis { get; set; } = new List<CompetenciaViewModel>();

    public List<FuncionarioCompetenciaViewModel> CompetenciasFuncionario { get; set; } = new List<FuncionarioCompetenciaViewModel>();
}