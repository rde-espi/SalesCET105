using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class AlterarRoleUserDTO
    {
        [Required]
        public string NovaRole { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Biografia { get; set; }

        public DateTime? DataAdmissao { get; set; }

        public bool Disponivel { get; set; } = true;
    }
}
