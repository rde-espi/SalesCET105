using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class RegistarDispositivoDTO
    {
        [Required]
        public string Fid { get; set; } = string.Empty;

        [Required]
        public string Plataforma { get; set; } = string.Empty;
    }
}
