namespace ProjetoFinalCet105.API.DTOs
{
    public class FaturaDTO
    {
        public int Id { get; set; }

        public int MarcacaoId { get; set; }
        public DateTime DataMarcacao { get; set; }

        public string Numero { get; set; } = string.Empty;

        public string Serie { get; set; } = string.Empty;

        public int NumeroSequencial { get; set; }

        public DateTime DataEmissao { get; set; }

        // Dados do cliente no momento da emissão
        public string? NomeCliente { get; set; }

        public string? NifCliente { get; set; }

        public string? MoradaCliente { get; set; }
        public string? CodigoPostalCliente { get; set; }
        public string? LocalidadeCliente { get; set; }

        // Valores
        public decimal Subtotal { get; set; }

        public decimal ValorDesconto { get; set; }

        public decimal ValorIva { get; set; }

        public decimal Total { get; set; }

        // Estado
        public string Estado { get; set; } = string.Empty;

        // Comunicação futura com AT
        public bool ComunicadaAT { get; set; }

        public DateTime? DataComunicacaoAT { get; set; }

        public string? CodigoRespostaAT { get; set; }

        public string? MensagemRespostaAT { get; set; }

        // Linhas da fatura
        public List<FaturaItemDTO> Itens { get; set; } = new();
    }
}
