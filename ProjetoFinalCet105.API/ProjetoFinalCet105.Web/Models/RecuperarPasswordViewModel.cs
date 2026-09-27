using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class RecuperarPasswordViewModel
{
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Introduza um endereço de email válido.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
}
