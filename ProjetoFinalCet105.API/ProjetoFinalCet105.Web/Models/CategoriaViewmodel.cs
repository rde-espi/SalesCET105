namespace ProjetoFinalCet105.Web.Models
{
    public class CategoriaViewModel
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public bool Ativa { get; set; }
    }
}