namespace ProjetoFinalCet105.Web.Models;

public class DashboardFuncionarioViewModel
{
    public FuncionarioViewModel Funcionario { get; set; } = new();

    public DateTime Data { get; set; } = DateTime.Today;

    public List<MarcacaoClienteViewModel> MarcacoesHoje { get; set; } = new();
    public int TotalMarcacoesHoje => MarcacoesHoje.Count;
    public int TotalPorFaturar { get; set; }
    public int TotalHoje { get; set; }

    public int TotalAmanha { get; set; }

    public int TotalEstaSemana { get; set; }
    public int MensagensNaoLidas { get; set; }

    public int NotificacoesNaoLidas { get; set; }
}