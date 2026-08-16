namespace ProjetoFinalCet105.API.Entities
{
    public class EstadoMarcacao:IEntity
    {
        public int Id { get; set; }

        public string Nome { get; set; }
        public string? Descricao { get; set; }

        public ICollection<Marcacao> Marcacoes { get; set; } = new List<Marcacao>();
    }
}
