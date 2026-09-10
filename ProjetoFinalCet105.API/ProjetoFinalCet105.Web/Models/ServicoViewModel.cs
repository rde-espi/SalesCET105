namespace ProjetoFinalCet105.Web.Models
{
    public class ServicoViewModel
    {
        public int Id { get; set; }

        public int CategoriaId { get; set; }

        public string? CategoriaNome { get; set; }

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