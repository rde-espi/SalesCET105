using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class UpdateClienteDTO
    {
        [Required]
        [MaxLength(150)]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefone { get; set; }

        [MaxLength(500)]
        public string? FotografiaUrl { get; set; }

        public bool? Ativo { get; set; }
    }
}
