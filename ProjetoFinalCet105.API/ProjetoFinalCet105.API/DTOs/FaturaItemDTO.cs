namespace ProjetoFinalCet105.API.DTOs
{
    public class FaturaItemDTO
    {
        public int Id { get; set; }

        public int? ServicoId { get; set; }

        public string Descricao { get; set; } = string.Empty;

        public decimal Quantidade { get; set; }

        public decimal PrecoUnitario { get; set; }

        public decimal PercentagemIva { get; set; }

        public decimal ValorIva { get; set; }

        public decimal Total { get; set; }

        public string? CodigoIva { get; set; }

        public string? MotivoIsencaoIva { get; set; }
    }
}
