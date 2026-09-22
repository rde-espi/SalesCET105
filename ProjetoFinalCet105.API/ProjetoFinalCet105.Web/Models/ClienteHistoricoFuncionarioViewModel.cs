namespace ProjetoFinalCet105.Web.Models;

public class ClienteHistoricoFuncionarioViewModel
{
    public string ClienteId { get; set; } = string.Empty;

    public string ClienteNome { get; set; } = string.Empty;

    public List<MarcacaoClienteViewModel> Historico { get; set; } = new List<MarcacaoClienteViewModel>();
}