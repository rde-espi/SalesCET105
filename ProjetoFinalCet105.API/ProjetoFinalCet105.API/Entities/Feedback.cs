namespace ProjetoFinalCet105.API.Entities
{
    public class Feedback:IEntity
    {
        public int Id { get; set; }

        public int MarcacaoId { get; set; }
        public Marcacao Marcacao { get; set; }

        public string ClienteId { get; set; }
        public User Cliente { get; set; }

        public int FuncionarioId { get; set; }
        public Funcionario Funcionario { get; set; }

        public int Classificacao { get; set; }

        public string? Comentario { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
