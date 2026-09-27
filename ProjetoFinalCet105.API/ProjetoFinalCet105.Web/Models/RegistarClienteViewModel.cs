using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class RegistarClienteViewModel
{
    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    [MaxLength(150)]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Introduza um email válido.")]
    [MaxLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A palavra-passe é obrigatória.")]
    [MinLength(6, ErrorMessage = "A palavra-passe deve ter pelo menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Palavra-passe")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a palavra-passe.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "As palavras-passe não coincidem.")]
    [Display(Name = "Confirmar palavra-passe")]
    public string ConfirmarPassword { get; set; } = string.Empty;

    [MaxLength(20)]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [RegularExpression(@"^\d{9}$",
        ErrorMessage = "O NIF deve conter exatamente 9 algarismos.")]
    [Display(Name = "NIF")]
    public string? Contribuinte { get; set; }

    [MaxLength(200)]
    [Display(Name = "Morada")]
    public string? Morada { get; set; }

    [MaxLength(20)]
    [Display(Name = "Código postal")]
    public string? CodigoPostal { get; set; }

    [MaxLength(100)]
    [Display(Name = "Localidade")]
    public string? Localidade { get; set; }

    [Display(Name = "Fotografia")]
    public IFormFile? Fotografia { get; set; }
}