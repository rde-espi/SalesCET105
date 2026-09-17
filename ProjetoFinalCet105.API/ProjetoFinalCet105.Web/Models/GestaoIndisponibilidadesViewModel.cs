namespace ProjetoFinalCet105.Web.Models;

public class GestaoIndisponibilidadesViewModel
{
    public List<IndisponibilidadeViewModel> Indisponibilidades { get; set; } = new();

    public List<FuncionarioViewModel> Funcionarios { get; set; } = new();

    public int Total { get; set; }

    public int Hoje { get; set; }

    public int Proximas { get; set; }

    public int DiasCompletos { get; set; }
}