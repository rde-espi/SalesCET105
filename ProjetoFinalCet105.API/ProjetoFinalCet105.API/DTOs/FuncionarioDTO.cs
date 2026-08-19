namespace ProjetoFinalCet105.API.DTOs
{
    public class FuncionarioDTO
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string NomeCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? FotografiaUrl { get; set; }

        public string? Biografia { get; set; }
        public DateTime? DataAdmissao { get; set; }

        public bool Disponivel { get; set; }
        public bool Ativo { get; set; }
    }
}
