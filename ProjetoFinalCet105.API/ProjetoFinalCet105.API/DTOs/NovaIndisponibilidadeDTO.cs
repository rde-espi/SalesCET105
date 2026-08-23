namespace ProjetoFinalCet105.API.DTOs
{
    public class NovaIndisponibilidadeDTO
    {
        public int? FuncionarioId { get; set; }

        public DateTime DataHoraInicio { get; set; }
        public DateTime DataHoraFim { get; set; }

        public string? Motivo { get; set; }

        public bool DiaCompleto { get; set; }
        public bool RestoDoDia { get; set; }
    }
}
