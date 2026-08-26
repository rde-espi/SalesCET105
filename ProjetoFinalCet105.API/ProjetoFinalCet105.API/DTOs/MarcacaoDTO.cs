namespace ProjetoFinalCet105.API.DTOs
{
    public class MarcacaoDTO
    {
        public int Id { get; set; }

        public string ClienteId { get; set; }
        public string ClienteNome { get; set; }

        public int FuncionarioId { get; set; }
        public string FuncionarioNome { get; set; }

        public int ServicoId { get; set; }
        public string ServicoNome { get; set; }

        public int EstadoMarcacaoId { get; set; }
        public string EstadoMarcacaoNome { get; set; }

        public DateTime DataHoraInicio { get; set; }
        public DateTime DataHoraFim { get; set; }

        public decimal Preco { get; set; }

        public string? Observacoes { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public int? PromoCodeId { get; set; }
        public string? PromoCode { get; set; }
        public decimal? PercentagemDescontoAplicada { get; set; }
        public decimal? ValorDesconto { get; set; }
    }
}
