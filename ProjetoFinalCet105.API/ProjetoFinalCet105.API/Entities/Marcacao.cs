namespace ProjetoFinalCet105.API.Entities
{
    public class Marcacao:IEntity
    {
        public int Id { get; set; }

        public string ClienteId { get; set; }
        public User Cliente { get; set; }

        public int FuncionarioId { get; set; }
        public Funcionario Funcionario { get; set; }

        public int ServicoId { get; set; }
        public Servico Servico { get; set; }

        public int EstadoMarcacaoId { get; set; }
        public EstadoMarcacao EstadoMarcacao { get; set; }

        public DateTime DataHoraInicio { get; set; }
        public DateTime DataHoraFim { get; set; }

        public decimal Preco { get; set; }

        public string? Observacoes { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
