using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class NovaConversaDTO
    {
        [Required]
        public string DestinatarioId { get; set; } = string.Empty;
    }
}
