namespace ProjetoFinalCet105.API.DTOs
{
    public class NovoFuncionarioDTO
    {
        // Dados do User
        public string NomeCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string? FotografiaUrl { get; set; }

        // Dados do Funcionario
        public string? Biografia { get; set; }
        public DateTime? DataAdmissao { get; set; }
        public bool Disponivel { get; set; }
    }
}
