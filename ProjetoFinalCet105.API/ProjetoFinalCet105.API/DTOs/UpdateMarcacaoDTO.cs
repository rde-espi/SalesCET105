namespace ProjetoFinalCet105.API.DTOs
{
    public class UpdateMarcacaoDTO
    {
        public int FuncionarioId { get; set; }

        public int ServicoId { get; set; }

        public DateTime DataHoraInicio { get; set; }

        public string? Observacoes { get; set; }
    }
}
