namespace ProjetoFinalCet105.API.DTOs
{
    public class NotificacaoDTO
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Mensagem { get; set; } = string.Empty;

        public bool Lida { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime? DataLeitura { get; set; }
    }
}
