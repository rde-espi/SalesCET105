using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class ContactoViewModel
{
    [Required(ErrorMessage = "Indique o seu nome.")]
    [StringLength(100)]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indique o seu email.")]
    [EmailAddress(ErrorMessage = "Introduza um endereço de email válido.")]
    [StringLength(150)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indique o assunto.")]
    [StringLength(150)]
    [Display(Name = "Assunto")]
    public string Assunto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escreva a sua mensagem.")]
    [StringLength(2000)]
    [Display(Name = "Mensagem")]
    public string Mensagem { get; set; } = string.Empty;
}