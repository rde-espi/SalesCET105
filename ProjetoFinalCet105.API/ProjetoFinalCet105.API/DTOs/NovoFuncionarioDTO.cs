using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class NovoFuncionarioDTO
    {
        // Dados do User

        [Required]
        [MaxLength(150)]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefone { get; set; }
        public IFormFile? Fotografia { get; set; }

        // Dados do Funcionario

        [MaxLength(1000)]
        public string? Biografia { get; set; }

        public DateTime? DataAdmissao { get; set; }

        public bool Disponivel { get; set; }
    }
}