using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class NovoHorarioFuncionarioDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "O funcionário indicado é inválido.")]
        public int? FuncionarioId { get; set; }

        public DayOfWeek DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFim { get; set; }
    }
}
