using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class PromoCodeViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O código é obrigatório.")]
    public string Codigo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    [Required(ErrorMessage = "A percentagem de desconto é obrigatória.")]
    [Range(0.01, 100, ErrorMessage = "O desconto deve estar entre 0 e 100%.")]
    public decimal PercentagemDesconto { get; set; }

    [Required]
    public DateTime DataInicio { get; set; }

    [Required]
    public DateTime DataFim { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "O limite deve ser superior a zero.")]
    public int? LimiteUtilizacoes { get; set; }

    public int NumeroUtilizacoes { get; set; }

    public bool Ativo { get; set; }
}