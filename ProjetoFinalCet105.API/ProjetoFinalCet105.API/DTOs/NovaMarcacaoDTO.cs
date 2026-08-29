using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class NovaMarcacaoDTO
    {
        public string? ClienteId { get; set; }

        public int? FuncionarioId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O serviço indicado é inválido.")]
        public int ServicoId { get; set; }

        public DateTime DataHoraInicio { get; set; }

        [MaxLength(500)]
        public string? Observacoes { get; set; }

        [MaxLength(50)]
        public string? PromoCode { get; set; }
    }
}
