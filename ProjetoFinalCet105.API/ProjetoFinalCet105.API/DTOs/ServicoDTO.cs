namespace ProjetoFinalCet105.API.DTOs
{
    public class ServicoDTO
    {
        public int Id { get; set; }

        public int CategoriaId { get; set; }
        public string CategoriaNome { get; set; } = string.Empty;

        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public int DuracaoMinutos { get; set; }
        public string? ImagemUrl { get; set; }
        public bool Disponivel { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
