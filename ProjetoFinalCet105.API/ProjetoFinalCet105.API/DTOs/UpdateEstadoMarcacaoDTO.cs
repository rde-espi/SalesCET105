using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class UpdateEstadoMarcacaoDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "O estado da marcação indicado é inválido.")]
        public int EstadoMarcacaoId { get; set; }
    }
}
