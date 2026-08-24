namespace ProjetoFinalCet105.API.DTOs
{
    public class UpdateHorarioFuncionarioDTO
    {

        public DayOfWeek DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }

        public bool? Ativo { get; set; }
    }
}
