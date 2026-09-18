using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class AlterarFotografiaPerfilViewModel
{
    public int FuncionarioId { get; set; }

    public string NomeCompleto { get; set; } = string.Empty;

    public string? FotografiaUrl { get; set; }

    public bool TemFotografia { get; set; }

    [Required(ErrorMessage = "Selecione uma fotografia.")]
    [Display(Name = "Fotografia")]
    public IFormFile? Fotografia { get; set; }
}
