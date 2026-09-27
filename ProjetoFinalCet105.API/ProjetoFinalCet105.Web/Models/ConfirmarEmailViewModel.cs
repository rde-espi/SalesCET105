using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class ConfirmarEmailViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Introduza o código recebido por email.")]
    [Display(Name = "Código de confirmação")]
    public string Codigo { get; set; } = string.Empty;
}
