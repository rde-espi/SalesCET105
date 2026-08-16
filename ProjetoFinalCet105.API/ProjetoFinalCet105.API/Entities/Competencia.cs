namespace ProjetoFinalCet105.API.Entities
{
    public class Competencia:IEntity
    {
        public int Id { get; set; }

        public string Nome { get; set; }
        public string? Descricao { get; set; }
        public bool Ativa { get; set; }

        public ICollection<FuncionarioCompetencia> FuncionarioCompetencias { get; set; } = new List<FuncionarioCompetencia>();
    }
}
