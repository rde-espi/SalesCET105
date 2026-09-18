namespace ProjetoFinalCet105.Web.Models;

public class MinhaAgendaViewModel
{
    public int FuncionarioId { get; set; }

    public string FuncionarioNome { get; set; } = string.Empty;

    public DateTime MesSelecionado { get; set; }

    public DateTime DataSelecionada { get; set; }

    public List<MarcacaoClienteViewModel> MarcacoesMes { get; set; } = new();
    public List<MarcacaoClienteViewModel> MarcacoesDia { get; set; } = new();

}