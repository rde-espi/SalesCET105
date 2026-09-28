using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs;

public class ContactoDTO
{
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Assunto { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Mensagem { get; set; } = string.Empty;
}