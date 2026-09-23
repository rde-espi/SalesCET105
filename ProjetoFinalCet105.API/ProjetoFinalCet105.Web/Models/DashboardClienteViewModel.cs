namespace ProjetoFinalCet105.Web.Models;

public class DashboardClienteViewModel
{
    public ClienteViewModel Cliente { get; set; } = new();

    public MarcacaoClienteViewModel? ProximaMarcacao { get; set; }

    public int TotalProximasMarcacoes { get; set; }

    public int TotalMarcacoesConcluidas { get; set; }

    public int MensagensNaoLidas { get; set; }

    public int NotificacoesNaoLidas { get; set; }
}
