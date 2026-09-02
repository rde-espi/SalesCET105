using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.Entities
{
    public class Fatura : IEntity
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        public string Serie { get; set; } = string.Empty;

        public int NumeroSequencial { get; set; }

        // Ligação à marcação que originou a fatura
        public int MarcacaoId { get; set; }
        public Marcacao Marcacao { get; set; } = null!;

        // Identificação do documento
        [Required]
        [MaxLength(50)]
        public string Numero { get; set; } = string.Empty;

        public DateTime DataEmissao { get; set; }

        // Snapshot do cliente no momento da emissão
        [MaxLength(150)]
        public string? NomeCliente { get; set; }

        [MaxLength(9)]
        public string? NifCliente { get; set; }

        [MaxLength(200)]
        public string? MoradaCliente { get; set; }

        [MaxLength(20)]
        public string? CodigoPostalCliente { get; set; }

        [MaxLength(100)]
        public string? LocalidadeCliente { get; set; }

        // Valores
        public decimal Subtotal { get; set; }

        public decimal ValorDesconto { get; set; }

        public decimal ValorIva { get; set; }

        public decimal Total { get; set; }

        // Estado do documento
        [Required]
        [MaxLength(30)]
        public string Estado { get; set; } = "Emitida";

        // Futuramente: integração AT
        public bool ComunicadaAT { get; set; }

        public DateTime? DataComunicacaoAT { get; set; }

        [MaxLength(100)]
        public string? CodigoRespostaAT { get; set; }

        [MaxLength(500)]
        public string? MensagemRespostaAT { get; set; }

        public ICollection<FaturaItem> Itens { get; set; }
            = new List<FaturaItem>();
    }
}
