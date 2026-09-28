namespace ProjetoFinalCet105.Web.Models;

public class ServicosPublicoViewModel
{
    public List<CategoriaViewModel> Categorias { get; set; } = new();

    public List<ServicoViewModel> Servicos { get; set; } = new();

    public int? CategoriaSelecionadaId { get; set; }

    public string? Pesquisa { get; set; }

    public CategoriaViewModel? CategoriaSelecionada => CategoriaSelecionadaId.HasValue ? Categorias.FirstOrDefault(c => c.Id == CategoriaSelecionadaId.Value) : null;
}