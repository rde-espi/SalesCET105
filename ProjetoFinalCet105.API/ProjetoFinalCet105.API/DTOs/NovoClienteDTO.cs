namespace ProjetoFinalCet105.API.DTOs
{
    public class NovoClienteDTO
    {
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Telefone { get; set; }
        public string? FotografiaUrl { get; set; }
    }
}
