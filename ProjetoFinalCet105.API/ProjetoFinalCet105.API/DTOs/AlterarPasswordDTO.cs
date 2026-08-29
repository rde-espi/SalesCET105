using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class AlterarPasswordDTO
    {
        [Required(ErrorMessage = "A password atual é obrigatória.")]
        public string PasswordAtual { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova password é obrigatória.")]
        [MinLength(6, ErrorMessage = "A nova password deve ter pelo menos 6 caracteres.")]
        public string NovaPassword { get; set; } = string.Empty;
    }
}
