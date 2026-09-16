using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class EditarMarcacaoViewModel
{
    public int Id { get; set; }

    [Required]
    public int ServicoId { get; set; }

    [Required]
    public DateTime DataHoraInicio { get; set; }

    [MaxLength(500)]
    public string? Observacoes { get; set; }

    public int FuncionarioId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string FuncionarioNome { get; set; } = string.Empty;

    public List<FuncionarioServicoViewModel> Servicos { get; set; } = new();
}