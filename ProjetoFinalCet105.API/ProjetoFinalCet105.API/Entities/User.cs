using Microsoft.AspNetCore.Identity;

namespace ProjetoFinalCet105.API.Entities
{
    public class User : IdentityUser 
    {
        public string NomeCompleto { get; set; }
        public string? Contribuinte { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Morada { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Localidade { get; set; }
        public string? FotografiaUrl { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public string? GoogleId { get; set; }
    }
}
