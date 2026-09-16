namespace ProjetoFinalCet105.Web.Models;

public class ClienteDetalhesViewModel
{
    public ClienteViewModel Cliente { get; set; } = new();

    public List<MarcacaoClienteViewModel> Marcacoes { get; set; } = new();
}