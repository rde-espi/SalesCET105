namespace ProjetoFinalCet105.API.DTOs
{
    public class DiaSemanaProcuraDTO
    {
        public int DiaSemana { get; set; }

        public string NomeDia { get; set; } = string.Empty;

        public int QuantidadeMarcacoes { get; set; }

        public decimal Percentagem { get; set; }
    }
}
