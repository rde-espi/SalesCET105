namespace ProjetoFinalCet105.API.Models;

public class ServicoFormModel
{
    public int CategoriaId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public decimal Preco { get; set; }

    public int DuracaoMinutos { get; set; }

    public IFormFile? Imagem { get; set; }
}