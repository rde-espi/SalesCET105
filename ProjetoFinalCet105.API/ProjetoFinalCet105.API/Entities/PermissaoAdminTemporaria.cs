namespace ProjetoFinalCet105.API.Entities
{
    public class PermissaoAdminTemporaria: IEntity
    {
        public int Id { get; set; }

        // Funcionário que recebe os privilégios
        public string FuncionarioUserId { get; set; } = string.Empty;

        public User FuncionarioUser { get; set; } = null!;

        // Admin permanente que concedeu os privilégios
        public string ConcedidoPorUserId { get; set; } = string.Empty;

        public User ConcedidoPorUser { get; set; } = null!;

        public DateTime DataInicio { get; set; }

        public DateTime DataFim { get; set; }

        public string? Motivo { get; set; }

        public bool Revogada { get; set; } = false;

        public DateTime? DataRevogacao { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataNotificacaoExpiracao { get; set; }
    }
}
