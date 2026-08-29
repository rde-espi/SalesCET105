using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class NovoFeedbackDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "A marcação indicada é inválida.")]
        public int MarcacaoId { get; set; }

        [Range(1, 5, ErrorMessage = "A classificação deve estar entre 1 e 5.")]
        public int Classificacao { get; set; }

        [MaxLength(1000)]
        public string? Comentario { get; set; }
    }
}
