namespace ProjetoFinalCet105.API.DTOs
{
    public class UpdateFuncionarioDTO
    {
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string? Telefone { get; set; }
        public string? FotografiaUrl { get; set; }
        public string? Biografia { get; set; }

        public bool? Disponivel { get; set; }

        public DateTime? DataAdmissao { get; set; }
        public bool? Ativo { get; set; }
    }
}
