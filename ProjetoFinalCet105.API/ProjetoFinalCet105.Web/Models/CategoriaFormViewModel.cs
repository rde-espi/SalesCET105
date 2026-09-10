using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.Web.Models
{
    public class CategoriaFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [Display(Name = "Imagem")]
        public IFormFile? Imagem { get; set; }

        // Usado na edição para sabermos se já existe imagem.
        public bool TemImagem { get; set; }
    }
}