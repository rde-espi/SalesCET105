using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class EditarPerfilFuncionarioViewModel
{
    public int FuncionarioId { get; set; }

    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    [MaxLength(150)]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Introduza um email válido.")]
    [MaxLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Biografia")]
    public string? Biografia { get; set; }

    [Display(Name = "Disponível para marcações")]
    public bool Disponivel { get; set; }
}