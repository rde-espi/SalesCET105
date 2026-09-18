using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class VerificarTwoFactorViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Introduza o código de autenticação.")]
    [Display(Name = "Código de autenticação")]
    public string Codigo { get; set; } = string.Empty;
}
