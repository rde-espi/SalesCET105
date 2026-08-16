namespace ProjetoFinalCet105.API.Entities
{
    public class Categoria : IEntity
    {
        public int Id { get; set; }

        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public bool Ativa { get; set; }

        public ICollection<Servico> Servicos { get; set; } = new List<Servico>();
    }
}
