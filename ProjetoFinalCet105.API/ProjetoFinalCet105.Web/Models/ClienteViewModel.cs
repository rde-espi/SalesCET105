namespace ProjetoFinalCet105.Web.Models;

public class ClienteViewModel
{
    public string Id { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Contribuinte { get; set; }
    public string? Morada { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Localidade { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}