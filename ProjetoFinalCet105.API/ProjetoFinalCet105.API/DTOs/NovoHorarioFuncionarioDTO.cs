namespace ProjetoFinalCet105.API.DTOs
{
    public class NovoHorarioFuncionarioDTO
    {
        public int? FuncionarioId { get; set; }

        public DayOfWeek DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }
    }
}
