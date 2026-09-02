namespace ProjetoFinalCet105.API.Services.Faturacao
{
    public class FaturacaoSettings
    {
        public const string SectionName = "Faturacao";

        public string Serie { get; set; } = string.Empty;

        public decimal TaxaIva { get; set; }

        public string CodigoIva { get; set; } = string.Empty;
        public bool PrecosIncluemIva { get; set; } = true;
        public string LogoPath { get; set; } = string.Empty;


        public string EmitenteNome { get; set; } = string.Empty;
        public string EmitenteNif { get; set; } = string.Empty;
        public string EmitenteMorada { get; set; } = string.Empty;
        public string EmitenteCodigoPostal { get; set; } = string.Empty;
        public string EmitenteLocalidade { get; set; } = string.Empty;
    }
}
