namespace ProjetoFinalCet105.API.DTOs
{
    public class HistoricoMarcacaoDTO
    {
        public int Id { get; set; }

        public int MarcacaoId { get; set; }

        public string UserId { get; set; }
        public string UserNome { get; set; }

        public string Acao { get; set; }
        public string? Descricao { get; set; }

        public DateTime DataAlteracao { get; set; }
    }
}
