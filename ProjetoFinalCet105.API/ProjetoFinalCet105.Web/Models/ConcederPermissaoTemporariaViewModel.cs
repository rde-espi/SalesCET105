using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class ConcederPermissaoTemporariaViewModel
{
    public string FuncionarioUserId { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int DuracaoMinutos { get; set; }

    [MaxLength(500)]
    public string? Motivo { get; set; }
}
