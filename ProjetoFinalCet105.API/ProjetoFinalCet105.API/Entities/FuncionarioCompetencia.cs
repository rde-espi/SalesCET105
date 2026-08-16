namespace ProjetoFinalCet105.API.Entities
{
    public class FuncionarioCompetencia:IEntity
    {
        public int Id { get; set; }
        public int FuncionarioId { get; set; }
        public Funcionario Funcionario { get; set; }

        public int CompetenciaId { get; set; }
        public Competencia Competencia { get; set; }

        public string? Nivel { get; set; }
        public string? Certificacao { get; set; }
    }
}
