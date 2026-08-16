namespace ProjetoFinalCet105.API.Entities
{
    public class HistoricoMarcacao:IEntity
    {
        public int Id { get; set; }

        public int MarcacaoId { get; set; }
        public Marcacao Marcacao { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public string Acao { get; set; }
        public string? Descricao { get; set; }

        public DateTime DataAlteracao { get; set; }
    }
}
