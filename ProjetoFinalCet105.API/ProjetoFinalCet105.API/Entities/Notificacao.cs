namespace ProjetoFinalCet105.API.Entities
{
    public class Notificacao:IEntity
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public string Titulo { get; set; }
        public string Mensagem { get; set; }

        public bool Lida { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataLeitura { get; set; }
    }
}
