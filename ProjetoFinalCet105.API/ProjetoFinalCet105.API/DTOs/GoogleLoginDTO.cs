using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class GoogleLoginDTO
    {
        [Required(ErrorMessage = "O token Google é obrigatório.")]
        public string IdToken { get; set; } = string.Empty;
    }
}
