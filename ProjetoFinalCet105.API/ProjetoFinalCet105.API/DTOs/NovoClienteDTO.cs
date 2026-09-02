using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class NovoClienteDTO
    {
        [Required]
        [MaxLength(150)]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefone { get; set; }

        [RegularExpression(@"^\d{9}$",ErrorMessage = "O NIF deve conter exatamente 9 algarismos.")]
        public string? Contribuinte { get; set; }
        [MaxLength(200)]
        public string? Morada { get; set; }

        [MaxLength(20)]
        public string? CodigoPostal { get; set; }

        [MaxLength(100)]
        public string? Localidade { get; set; }

        [MaxLength(500)]
        public string? FotografiaUrl { get; set; }

    }
}
