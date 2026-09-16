namespace ProjetoFinalCet105.Web.Models;

public class AlterarRoleViewModel
{
    public string NovaRole { get; set; } = string.Empty;

    public string? Biografia { get; set; }

    public DateTime? DataAdmissao { get; set; }

    public bool Disponivel { get; set; } = true;
}
