using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Introduza um email válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A password é obrigatória.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}