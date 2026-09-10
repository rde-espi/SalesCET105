namespace ProjetoFinalCet105.API.Models
{
    public class CategoriaFormModel
    {
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public IFormFile? Imagem { get; set; }
    }
}