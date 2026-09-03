namespace ProjetoFinalCet105.API.DTOs
{
    public class CategoriaFaturacaoDTO
    {
        public int CategoriaId { get; set; }

        public string NomeCategoria { get; set; } = string.Empty;

        public int QuantidadeServicos { get; set; }

        public decimal TotalFaturado { get; set; }
    }
}
