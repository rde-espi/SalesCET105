using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class FuncionarioFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Introduza um email válido.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [Display(Name = "Biografia")]
    public string? Biografia { get; set; }

    [Display(Name = "Data de admissão")]
    [DataType(DataType.Date)]
    public DateTime? DataAdmissao { get; set; }

    [Display(Name = "Disponível")]
    public bool Disponivel { get; set; }

    public bool Ativo { get; set; }
    public List<FuncionarioServicoSelecaoViewModel> Servicos { get; set; } = new();
    public List<CompetenciaViewModel> CompetenciasDisponiveis { get; set; } = new();

    public List<FuncionarioCompetenciaViewModel> CompetenciasFuncionario { get; set; } = new();
    public string UserId { get; set; } = string.Empty;

    public string RoleAtual { get; set; } = "Funcionario";

    public PermissaoAdminTemporariaViewModel? PermissaoAdminTemporariaAtiva { get; set; }
    public List<MeuHorarioViewModel> Horarios { get; set; } = new();
}