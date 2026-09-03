namespace ProjetoFinalCet105.API.DTOs
{
    public class ServicoMaisMarcadoDTO
    {
        public int ServicoId { get; set; }

        public string NomeServico { get; set; } = string.Empty;

        public int QuantidadeMarcacoes { get; set; }

        public decimal Percentagem { get; set; }
    }
}
