namespace ProjetoFinalCet105.API.DTOs
{
    public class PermissaoAdminTemporariaDTO
    {
        public int Id { get; set; }

        public string FuncionarioUserId { get; set; } = string.Empty;
        public string FuncionarioNome { get; set; } = string.Empty;

        public string ConcedidoPorUserId { get; set; } = string.Empty;
        public string ConcedidoPorNome { get; set; } = string.Empty;
        public string? RevogadaPorUserId { get; set; }

        public string? RevogadaPorNome { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        public string? Motivo { get; set; }

        public bool Revogada { get; set; }
        public DateTime? DataRevogacao { get; set; }

        public string Estado { get; set; } = string.Empty;
    }
}
