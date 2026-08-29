using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class FuncionarioCompetenciaDTO
    {
        public int Id { get; set; }

        public int FuncionarioId { get; set; }

        public string FuncionarioNome { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "A competência indicada é inválida.")]
        public int CompetenciaId { get; set; }

        public string CompetenciaNome { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Nivel { get; set; }

        [MaxLength(500)]
        public string? Certificacao { get; set; }
    }
}
