namespace ProjetoFinalCet105.API.Entities
{
    public class Servico:IEntity
    {
        public int Id { get; set; }

        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }

        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public int DuracaoMinutos { get; set; }
        public string? ImagemUrl { get; set; }
        public bool Disponivel { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        public ICollection<FuncionarioServico> FuncionarioServicos { get; set; } = new List<FuncionarioServico>();
    }
}
