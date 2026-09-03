namespace ProjetoFinalCet105.API.DTOs
{
    public class FaturacaoMensalDTO
    {
        public int Ano { get; set; }

        public int Mes { get; set; }

        public string NomeMes { get; set; } = string.Empty;

        public decimal Total { get; set; }
    }
}
