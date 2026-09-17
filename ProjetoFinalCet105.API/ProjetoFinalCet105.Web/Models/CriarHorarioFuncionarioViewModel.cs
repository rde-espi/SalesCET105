using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class CriarHorarioFuncionarioViewModel
{
    [Required]
    public int FuncionarioId { get; set; }

    [Required(ErrorMessage = "Selecione o dia da semana.")]
    public DayOfWeek DiaSemana { get; set; }

    [Required(ErrorMessage = "Indique a hora de início.")]
    public TimeSpan HoraInicio { get; set; }

    [Required(ErrorMessage = "Indique a hora de fim.")]
    public TimeSpan HoraFim { get; set; }
}
