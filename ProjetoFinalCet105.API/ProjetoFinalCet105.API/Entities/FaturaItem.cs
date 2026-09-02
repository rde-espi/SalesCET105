using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.Entities
{
    public class FaturaItem : IEntity
    {
        public int Id { get; set; }

        public int FaturaId { get; set; }
        public Fatura Fatura { get; set; } = null!;
        [MaxLength(10)]
        public string? CodigoIva { get; set; }

        [MaxLength(200)]
        public string? MotivoIsencaoIva { get; set; }

        // Snapshot do serviço
        public int? ServicoId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Descricao { get; set; } = string.Empty;

        public decimal Quantidade { get; set; } = 1;

        public decimal PrecoUnitario { get; set; }

        public decimal PercentagemIva { get; set; }

        public decimal ValorIva { get; set; }

        public decimal Total { get; set; }
    }
}
