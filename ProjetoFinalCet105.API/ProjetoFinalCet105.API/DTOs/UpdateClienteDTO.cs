namespace ProjetoFinalCet105.API.DTOs
{
    public class UpdateClienteDTO
    {
        public string NomeCompleto { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Telefone { get; set; }

        public string? FotografiaUrl { get; set; }

        public bool? Ativo { get; set; }
    }
}
