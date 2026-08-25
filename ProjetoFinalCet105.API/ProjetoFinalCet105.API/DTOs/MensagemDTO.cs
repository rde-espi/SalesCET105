namespace ProjetoFinalCet105.API.DTOs
{
    public class MensagemDTO
    {
        public int Id { get; set; }

        public int ConversaId { get; set; }

        public string RemetenteId { get; set; } = string.Empty;
        public string RemetenteNome { get; set; } = string.Empty;

        public string Texto { get; set; } = string.Empty;

        public DateTime DataEnvio { get; set; }

        public bool Lida { get; set; }
        public DateTime? DataLeitura { get; set; }
    }
}
