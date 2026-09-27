using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class ResetPasswordViewModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "A nova palavra-passe é obrigatória.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova palavra-passe")]
    public string NovaPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova palavra-passe.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar palavra-passe")]
    [Compare(
        nameof(NovaPassword),
        ErrorMessage = "As palavras-passe não coincidem.")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}