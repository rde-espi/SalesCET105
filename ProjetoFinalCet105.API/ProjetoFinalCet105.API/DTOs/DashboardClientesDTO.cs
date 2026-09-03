namespace ProjetoFinalCet105.API.DTOs
{
    public class DashboardClientesDTO
    {
        public int TotalClientes { get; set; }

        public int NovosClientesMes { get; set; }

        public int ClientesRecorrentes { get; set; }

        public int ClientesInativos60Dias { get; set; }

        public int ClientesInativos90Dias { get; set; }

        public decimal TaxaRecorrencia { get; set; }
    }
}
