using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class NovaMarcacaoClienteViewModel
{
    [Required(ErrorMessage = "Selecione o serviço.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione o serviço.")]
    public int ServicoId { get; set; }

    [Required(ErrorMessage = "Selecione o profissional.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione o profissional.")]
    public int FuncionarioId { get; set; }

    [Required(ErrorMessage = "Selecione a data e o horário.")]
    public DateTime DataHoraInicio { get; set; }

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    [MaxLength(50)]
    public string? PromoCode { get; set; }

    public List<CategoriaViewModel> Categorias { get; set; } = new();

    public List<ServicoViewModel> Servicos { get; set; } = new();

    public List<FuncionarioViewModel> Funcionarios { get; set; } = new();
}