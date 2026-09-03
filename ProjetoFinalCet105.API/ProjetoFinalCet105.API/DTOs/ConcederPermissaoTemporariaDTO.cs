using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class ConcederPermissaoAdminTemporariaDTO
    {
        [Required]
        public string FuncionarioUserId { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int DuracaoMinutos { get; set; }

        [MaxLength(500)]
        public string? Motivo { get; set; }
    }
}
