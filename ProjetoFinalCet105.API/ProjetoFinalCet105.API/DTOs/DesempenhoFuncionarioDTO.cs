namespace ProjetoFinalCet105.API.DTOs
{
    public class DesempenhoFuncionarioDTO
    {
        public int FuncionarioId { get; set; }

        public string NomeFuncionario { get; set; } = string.Empty;

        public int MarcacoesConcluidas { get; set; }

        public decimal TotalFaturado { get; set; }

        public decimal AvaliacaoMedia { get; set; }
    }
}
