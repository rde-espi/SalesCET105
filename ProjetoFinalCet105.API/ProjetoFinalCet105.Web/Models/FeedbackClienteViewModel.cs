using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class FeedbackClienteViewModel
{
    public int? Id { get; set; }

    public int MarcacaoId { get; set; }

    public string ServicoNome { get; set; } = string.Empty;

    public string FuncionarioNome { get; set; } = string.Empty;

    public DateTime DataHoraInicio { get; set; }

    [Required(ErrorMessage = "Selecione uma classificação.")]
    [Range(1, 5, ErrorMessage = "A classificação deve estar entre 1 e 5.")]
    public int Classificacao { get; set; }

    [StringLength(1000, ErrorMessage = "O comentário não pode exceder 1000 caracteres.")]
    public string? Comentario { get; set; }

    public DateTime? DataCriacao { get; set; }
}
