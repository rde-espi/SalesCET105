using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class PerfilViewModel
{
    // Identificação
    public string UserId { get; set; } = string.Empty;

    public string TipoUtilizador { get; set; } = string.Empty;

    // Dados pessoais
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [Display(Name = "NIF")]
    public string? Contribuinte { get; set; }

    [Display(Name = "Data de nascimento")]
    public DateTime? DataNascimento { get; set; }

    [Display(Name = "Morada")]
    public string? Morada { get; set; }

    [Display(Name = "Código postal")]
    public string? CodigoPostal { get; set; }

    [Display(Name = "Localidade")]
    public string? Localidade { get; set; }

    // Campos específicos do funcionário
    [Display(Name = "Biografia")]
    public string? Biografia { get; set; }

    [Display(Name = "Disponível")]
    public bool? Disponivel { get; set; }

    // Fotografia
    public string? FotografiaUrl { get; set; }

    [Display(Name = "Nova fotografia")]
    public IFormFile? NovaFotografia { get; set; }
    public bool TemFotografia { get; set; }

    // Integrações
    public bool GoogleCalendarLigado { get; set; }

    public string? GoogleEmail { get; set; }
}
