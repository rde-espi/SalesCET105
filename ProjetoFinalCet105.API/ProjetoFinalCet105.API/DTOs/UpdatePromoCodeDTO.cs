namespace ProjetoFinalCet105.API.DTOs
{
    public class UpdatePromoCodeDTO
    {
        public string Codigo { get; set; }
        public string? Descricao { get; set; }

        public decimal PercentagemDesconto { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        public int? LimiteUtilizacoes { get; set; }
    }
}
