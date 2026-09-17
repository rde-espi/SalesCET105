using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class NovaMarcacaoAdminViewModel
{
    [Required(ErrorMessage = "Selecione o cliente.")]
    public string ClienteId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione o serviço.")]
    public int ServicoId { get; set; }

    [Required(ErrorMessage = "Selecione o profissional.")]
    public int FuncionarioId { get; set; }

    [Required(ErrorMessage = "Selecione a data e o horário.")]
    public DateTime DataHoraInicio { get; set; }

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    public List<ClienteViewModel> Clientes { get; set; } = new();
    public List<ServicoViewModel> Servicos { get; set; } = new();
    public List<FuncionarioViewModel> Funcionarios { get; set; } = new();
}
