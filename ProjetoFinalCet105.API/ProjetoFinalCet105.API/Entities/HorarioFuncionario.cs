namespace ProjetoFinalCet105.API.Entities
{
    public class HorarioFuncionario:IEntity
    {
        public int Id { get; set; }

        public int FuncionarioId { get; set; }
        public Funcionario Funcionario { get; set; }

        public DayOfWeek DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFim { get; set; }

        public bool Ativo { get; set; }
    }
}
