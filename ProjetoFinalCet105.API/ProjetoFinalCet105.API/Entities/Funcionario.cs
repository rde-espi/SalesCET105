namespace ProjetoFinalCet105.API.Entities
{
    public class Funcionario:IEntity
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public string? Biografia { get; set; }
        public DateTime? DataAdmissao { get; set; }
        public bool Disponivel { get; set; }
        public bool Ativo { get; set; }

        public ICollection<FuncionarioServico> FuncionarioServicos { get; set; } = new List<FuncionarioServico>();
        public ICollection<FuncionarioCompetencia> FuncionarioCompetencias { get; set; } = new List<FuncionarioCompetencia>();
        public ICollection<HorarioFuncionario> Horarios { get; set; } = new List<HorarioFuncionario>();
        public ICollection<Indisponibilidade> Indisponibilidades { get; set; } = new List<Indisponibilidade>();
    }
}
