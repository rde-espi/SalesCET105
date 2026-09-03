namespace ProjetoFinalCet105.API.DTOs
{
    public class DespesaDTO
    {
        public int Id { get; set; }

        public string Descricao { get; set; } = string.Empty;

        public decimal Valor { get; set; }

        public DateTime DataDespesa { get; set; }

        public string? Categoria { get; set; }

        public string? Observacoes { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
