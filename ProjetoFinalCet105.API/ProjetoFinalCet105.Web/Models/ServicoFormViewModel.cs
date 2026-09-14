using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models;

public class ServicoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    [Display(Name = "Categoria")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "O nome do serviço é obrigatório.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "O preço não pode ser negativo.")]
    [Display(Name = "Preço")]
    public decimal Preco { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A duração deve ser superior a zero.")]
    [Display(Name = "Duração")]
    public int DuracaoMinutos { get; set; }

    [Display(Name = "Imagem")]
    public IFormFile? Imagem { get; set; }

    public bool TemImagem { get; set; }

    public List<CategoriaViewModel> Categorias { get; set; } = new();
}