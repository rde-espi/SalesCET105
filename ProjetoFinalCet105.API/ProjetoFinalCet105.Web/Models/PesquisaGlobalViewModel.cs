namespace ProjetoFinalCet105.Web.Models;

public class PesquisaGlobalViewModel
{
    public string Termo { get; set; } = string.Empty;

    public List<ClienteViewModel> Clientes { get; set; } = new();
    public List<ServicoViewModel> Servicos { get; set; } = new();
    public List<MarcacaoClienteViewModel> Marcacoes { get; set; } = new();
    public List<FuncionarioViewModel> Funcionarios { get; set; } = new();

    public int TotalResultados =>
        Clientes.Count +
        Servicos.Count +
        Marcacoes.Count +
        Funcionarios.Count;
}
