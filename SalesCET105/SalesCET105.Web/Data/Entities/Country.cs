using System.ComponentModel.DataAnnotations;

namespace SalesCET105.Web.Data.Entities
{
    public class Country
    {
        public int Id { get; set; }

        [Display(Name="País")]
        [MaxLength(50,ErrorMessage ="O campo {0} deve ter no maximo {1} caracteres!")]
        [Required]
        public string? Name { get; set; }
    }
}
