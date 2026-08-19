namespace ProjetoFinalCet105.API.DTOs
{
    public class IndisponibilidadeDTO
    {
        public int Id { get; set; }

        public int FuncionarioId { get; set; }
        public string FuncionarioNome { get; set; } = string.Empty;

        public DateTime DataHoraInicio { get; set; }
        public DateTime DataHoraFim { get; set; }

        public string? Motivo { get; set; }

        public bool DiaCompleto { get; set; }
    }
}
