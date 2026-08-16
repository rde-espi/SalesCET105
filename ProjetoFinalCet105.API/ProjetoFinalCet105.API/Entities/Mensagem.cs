namespace ProjetoFinalCet105.API.Entities
{
    public class Mensagem:IEntity
    {
        public int Id { get; set; }

        public int ConversaId { get; set; }
        public Conversa Conversa { get; set; }

        public string RemetenteId { get; set; }
        public User Remetente { get; set; }

        public string Texto { get; set; }

        public DateTime DataEnvio { get; set; }

        public bool Lida { get; set; }
        public DateTime? DataLeitura { get; set; }
    }
}
