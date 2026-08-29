using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class UpdateMarcacaoDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "O serviço indicado é inválido.")]
        public int ServicoId { get; set; }

        public DateTime DataHoraInicio { get; set; }

        [MaxLength(500)]
        public string? Observacoes { get; set; }
    }
}
