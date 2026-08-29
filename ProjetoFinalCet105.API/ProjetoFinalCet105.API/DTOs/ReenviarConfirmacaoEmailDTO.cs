using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class ReenviarConfirmacaoEmailDTO
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O email indicado não é válido.")]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;
    }
}
