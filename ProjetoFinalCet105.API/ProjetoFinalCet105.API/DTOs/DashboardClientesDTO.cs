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

    public class DashboardClienteRecenteDTO
    {
        public string Id { get; set; } = string.Empty;
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}