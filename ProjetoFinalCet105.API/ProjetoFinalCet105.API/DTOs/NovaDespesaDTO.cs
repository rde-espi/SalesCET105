using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class NovaDespesaDTO
    {
        [Required]
        [MaxLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Range(0.01, 999999999)]
        public decimal Valor { get; set; }

        public DateTime DataDespesa { get; set; }

        [MaxLength(100)]
        public string? Categoria { get; set; }

        [MaxLength(500)]
        public string? Observacoes { get; set; }
    }
}
