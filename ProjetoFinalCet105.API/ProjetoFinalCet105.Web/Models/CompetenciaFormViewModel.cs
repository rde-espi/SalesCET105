using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class CompetenciaFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome da competência é obrigatório.")]
    [MaxLength(150)]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    [Display(Name = "Ativa")]
    public bool Ativa { get; set; } = true;
}