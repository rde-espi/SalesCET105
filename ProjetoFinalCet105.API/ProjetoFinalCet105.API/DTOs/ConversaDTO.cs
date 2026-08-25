namespace ProjetoFinalCet105.API.DTOs
{
    public class ConversaDTO
    {
        public int Id { get; set; }

        public string ClienteId { get; set; } = string.Empty;
        public string ClienteNome { get; set; } = string.Empty;

        public string FuncionarioUserId { get; set; } = string.Empty;
        public string FuncionarioNome { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; }

        public List<MensagemDTO> Mensagens { get; set; } = new List<MensagemDTO>();
    }
}
