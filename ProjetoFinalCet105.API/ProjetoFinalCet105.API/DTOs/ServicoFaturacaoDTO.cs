namespace ProjetoFinalCet105.API.DTOs
{
    public class ServicoFaturacaoDTO
    {
        public int? ServicoId { get; set; }

        public string NomeServico { get; set; } = string.Empty;

        public int Quantidade { get; set; }

        public decimal TotalFaturado { get; set; }
    }
}
