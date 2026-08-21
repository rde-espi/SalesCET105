namespace ProjetoFinalCet105.API.DTOs
{
    public class FeedbackDTO
    {
        public int Id { get; set; }

        public int MarcacaoId { get; set; }

        public string ClienteId { get; set; }
        public string ClienteNome { get; set; }

        public int FuncionarioId { get; set; }
        public string FuncionarioNome { get; set; }

        public int Classificacao { get; set; }

        public string? Comentario { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
