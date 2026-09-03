namespace ProjetoFinalCet105.API.DTOs
{
    public class DashboardAgendaDTO
    {
        public int TotalMarcacoesMes { get; set; }

        public int MarcacoesConcluidasMes { get; set; }

        public int MarcacoesCanceladasMes { get; set; }

        public int NaoCompareceuMes { get; set; }

        public int MarcacoesPendentesMes { get; set; }

        public int MarcacoesConfirmadasMes { get; set; }

        public decimal TaxaConclusao { get; set; }

        public decimal TaxaCancelamento { get; set; }

        public decimal TaxaNaoComparecimento { get; set; }


        public decimal HorasDisponiveisMes { get; set; }

        public decimal HorasOcupadasMes { get; set; }

        public decimal TaxaOcupacao { get; set; }
        public decimal HorasLivresMes { get; set; }
    }
}
