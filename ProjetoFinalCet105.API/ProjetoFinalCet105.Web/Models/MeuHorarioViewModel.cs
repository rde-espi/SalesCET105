namespace ProjetoFinalCet105.Web.Models;

public class MeuHorarioViewModel
{
    public int Id { get; set; }

    public int FuncionarioId { get; set; }

    public string FuncionarioNome { get; set; } = string.Empty;

    public DayOfWeek DiaSemana { get; set; }

    public TimeSpan HoraInicio { get; set; }

    public TimeSpan HoraFim { get; set; }

    public bool Ativo { get; set; }
}
