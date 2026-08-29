using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class VerificarTwoFactorDTO
    {
        [Required(ErrorMessage = "O utilizador é obrigatório.")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "O código de verificação é obrigatório.")]
        public string Codigo { get; set; } = string.Empty;
    }
}
