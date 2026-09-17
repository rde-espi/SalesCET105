using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class DespesaViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O valor é obrigatório.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser superior a zero.")]
    public decimal Valor { get; set; }

    [Required(ErrorMessage = "A data da despesa é obrigatória.")]
    public DateTime DataDespesa { get; set; }

    [StringLength(100)]
    public string? Categoria { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    public DateTime DataCriacao { get; set; }
}