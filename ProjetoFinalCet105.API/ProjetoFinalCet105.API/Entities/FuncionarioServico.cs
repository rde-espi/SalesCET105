namespace ProjetoFinalCet105.API.Entities
{
    public class FuncionarioServico:IEntity
    {
        public int Id { get; set; }
        public int FuncionarioId { get; set; }
        public Funcionario Funcionario { get; set; }

        public int ServicoId { get; set; }
        public Servico Servico { get; set; }

        public decimal? PrecoPersonalizado { get; set; }
        public int? DuracaoPersonalizadaMinutos { get; set; }

        public bool Ativo { get; set; }
    }
}
