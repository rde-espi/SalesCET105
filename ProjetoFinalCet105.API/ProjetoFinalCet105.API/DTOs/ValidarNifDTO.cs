using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class ValidarNifDTO
    {
        [Required(ErrorMessage = "O NIF é obrigatório.")]
        [RegularExpression( @"^\d{9}$", ErrorMessage = "O NIF deve conter exatamente 9 algarismos.")]
        public string Nif { get; set; } = string.Empty;
    }
}
