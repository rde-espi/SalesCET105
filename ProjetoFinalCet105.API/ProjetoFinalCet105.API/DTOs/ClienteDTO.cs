namespace ProjetoFinalCet105.API.DTOs
{
    public class ClienteDTO
    {
        public string Id { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string? Telefone { get; set; }
        public string? FotografiaUrl { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
