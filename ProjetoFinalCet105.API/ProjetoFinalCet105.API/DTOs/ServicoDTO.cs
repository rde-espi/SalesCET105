using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class ServicoDTO
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A categoria indicada é inválida.")]
        public int CategoriaId { get; set; }

        public string CategoriaNome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome do serviço é obrigatório.")]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descricao { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999",
            ErrorMessage = "O preço do serviço não pode ser negativo.")]
        public decimal Preco { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "A duração do serviço deve ser superior a zero.")]
        public int DuracaoMinutos { get; set; }

        [MaxLength(500)]
        public string? ImagemUrl { get; set; }

        public bool Disponivel { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime? DataAtualizacao { get; set; }
    }
}
