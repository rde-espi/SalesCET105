namespace ProjetoFinalCet105.API.DTOs
{
    public class DashboardFinanceiroDTO
    {
        public decimal FaturacaoHoje { get; set; }

        public decimal FaturacaoSemana { get; set; }

        public decimal FaturacaoMes { get; set; }

        public int TotalFaturasMes { get; set; }

        public decimal TicketMedioMes { get; set; }

        public int FaturasComNifMes { get; set; }

        public int FaturasSemNifMes { get; set; }

        public decimal ValorComNifMes { get; set; }

        public decimal ValorSemNifMes { get; set; }

        public decimal DespesasMes { get; set; }

        public decimal ResultadoMes { get; set; }

        public decimal MargemPercentualMes { get; set; }
    }
}
