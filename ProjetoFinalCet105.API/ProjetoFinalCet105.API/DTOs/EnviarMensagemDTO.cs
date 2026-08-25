using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class EnviarMensagemDTO
    {
        [Required]
        [MaxLength(2000)]
        public string Texto { get; set; } = string.Empty;
    }
}
