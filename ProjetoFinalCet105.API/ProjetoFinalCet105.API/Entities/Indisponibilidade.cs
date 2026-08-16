namespace ProjetoFinalCet105.API.Entities
{
    public class Indisponibilidade:IEntity
    {
        public int Id { get; set; }

        public int FuncionarioId { get; set; }
        public Funcionario Funcionario { get; set; }

        public DateTime DataHoraInicio { get; set; }
        public DateTime DataHoraFim { get; set; }

        public string? Motivo { get; set; }

        public bool DiaCompleto { get; set; }
    }
}
