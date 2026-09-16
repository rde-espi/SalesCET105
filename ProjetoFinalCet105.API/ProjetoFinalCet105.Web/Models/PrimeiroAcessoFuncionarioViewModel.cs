using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class PrimeiroAcessoFuncionarioViewModel
{
    public string Email { get; set; } = string.Empty;

    public string TokenConfirmacao { get; set; } = string.Empty;

    public string TokenPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "A palavra-passe é obrigatória.")]
    [MinLength(6, ErrorMessage = "A palavra-passe deve ter pelo menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova palavra-passe")]
    public string NovaPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a palavra-passe.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NovaPassword), ErrorMessage = "As palavras-passe não coincidem.")]
    [Display(Name = "Confirmar palavra-passe")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}