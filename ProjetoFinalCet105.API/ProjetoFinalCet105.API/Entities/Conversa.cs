namespace ProjetoFinalCet105.API.Entities
{
    public class Conversa:IEntity
    {
        public int Id { get; set; }

        public string ClienteId { get; set; }
        public User Cliente { get; set; }

        public string FuncionarioUserId { get; set; }
        public User Funcionario { get; set; }

        public DateTime DataCriacao { get; set; }

        public ICollection<Mensagem> Mensagens { get; set; } = new List<Mensagem>();
    }
}
