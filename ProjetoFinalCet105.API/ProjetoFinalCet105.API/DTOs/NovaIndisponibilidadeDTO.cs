using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class NovaIndisponibilidadeDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "O funcionário indicado é inválido.")]
        public int? FuncionarioId { get; set; }

        public DateTime DataHoraInicio { get; set; }

        public DateTime DataHoraFim { get; set; }

        [MaxLength(500)]
        public string? Motivo { get; set; }

        public bool DiaCompleto { get; set; }

        public bool RestoDoDia { get; set; }
    }
}
