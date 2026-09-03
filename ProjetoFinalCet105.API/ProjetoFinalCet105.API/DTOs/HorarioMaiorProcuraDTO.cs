namespace ProjetoFinalCet105.API.DTOs
{
    public class HorarioMaiorProcuraDTO
    {
        public int Hora { get; set; }

        public string FaixaHoraria { get; set; } = string.Empty;

        public int QuantidadeMarcacoes { get; set; }

        public decimal Percentagem { get; set; }
    }
}
