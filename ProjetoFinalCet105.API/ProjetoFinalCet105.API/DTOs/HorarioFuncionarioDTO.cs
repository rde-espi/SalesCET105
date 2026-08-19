namespace ProjetoFinalCet105.API.DTOs
{
    public class HorarioFuncionarioDTO
    {
        public int Id { get; set; }

        public int FuncionarioId { get; set; }
        public string FuncionarioNome { get; set; } = string.Empty;

        public DayOfWeek DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }

        public bool Ativo { get; set; }
    }
}
