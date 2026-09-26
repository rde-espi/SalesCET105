using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class EditarPerfilClienteViewModel
{
    public string ClienteId { get; set; } = string.Empty;

    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    [MaxLength(150)]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Introduza um endereço de email válido.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [RegularExpression(@"^\d{9}$", ErrorMessage = "O NIF deve conter exatamente 9 algarismos.")]
    public string? Contribuinte { get; set; }

    [MaxLength(200)]
    public string? Morada { get; set; }

    [MaxLength(20)]
    public string? CodigoPostal { get; set; }

    [MaxLength(100)]
    public string? Localidade { get; set; }
}
