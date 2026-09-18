using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class AlterarPasswordViewModel
{
    [Required(ErrorMessage = "A palavra-passe atual é obrigatória.")]
    [DataType(DataType.Password)]
    [Display(Name = "Palavra-passe atual")]
    public string PasswordAtual { get; set; } = string.Empty;


    [Required(ErrorMessage = "A nova palavra-passe é obrigatória.")]
    [MinLength(
        6,
        ErrorMessage = "A nova palavra-passe deve ter pelo menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova palavra-passe")]
    public string NovaPassword { get; set; } = string.Empty;


    [Required(ErrorMessage = "Confirme a nova palavra-passe.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar nova palavra-passe")]
    [Compare(
        nameof(NovaPassword),
        ErrorMessage = "As palavras-passe não coincidem.")]
    public string ConfirmarNovaPassword { get; set; } = string.Empty;
}