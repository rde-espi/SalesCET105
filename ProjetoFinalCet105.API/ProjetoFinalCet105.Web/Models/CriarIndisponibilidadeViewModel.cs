using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class CriarIndisponibilidadeViewModel
{
    [Required(ErrorMessage = "Selecione o profissional.")]
    [Display(Name = "Profissional")]
    public int? FuncionarioId { get; set; }

    [Required(ErrorMessage = "Indique a data.")]
    [Display(Name = "Data")]
    public DateTime Data { get; set; } = DateTime.Today;

    [Display(Name = "Hora de início")]
    public TimeSpan? HoraInicio { get; set; }

    [Display(Name = "Hora de fim")]
    public TimeSpan? HoraFim { get; set; }

    [Display(Name = "Tipo")]
    public string Tipo { get; set; } = "periodo";

    [Display(Name = "Motivo")]
    [StringLength(500)]
    public string? Motivo { get; set; }

    public List<FuncionarioViewModel> Funcionarios { get; set; } = new();
}