namespace ProjetoFinalCet105.API.DTOs
{
    public class PromoCodeDTO
    {
        public int Id { get; set; }

        public string Codigo { get; set; }
        public string? Descricao { get; set; }

        public decimal PercentagemDesconto { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        public int? LimiteUtilizacoes { get; set; }
        public int NumeroUtilizacoes { get; set; }

        public bool Ativo { get; set; }
    }
}
