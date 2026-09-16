namespace ProjetoFinalCet105.Web.Models;

public class MarcacaoDetalhesViewModel
{
    public MarcacaoClienteViewModel Marcacao { get; set; } = new();

    public List<HistoricoMarcacaoViewModel> Historico { get; set; } = new();
}