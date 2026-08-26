namespace ProjetoFinalCet105.API.DTOs
{
    public class NovaMarcacaoDTO
    {
        public string? ClienteId { get; set; }

        public int? FuncionarioId { get; set; }

        public int ServicoId { get; set; }

        public DateTime DataHoraInicio { get; set; }

        public string? Observacoes { get; set; }
        public string? PromoCode { get; set; }
    }
}
