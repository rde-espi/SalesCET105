namespace ProjetoFinalCet105.API.DTOs
{
    public class FuncionarioServicoDTO
    {
        public int Id { get; set; }

        public int FuncionarioId { get; set; }
        public string FuncionarioNome { get; set; } = string.Empty;

        public int ServicoId { get; set; }
        public string ServicoNome { get; set; } = string.Empty;

        public decimal? PrecoPersonalizado { get; set; }
        public int? DuracaoPersonalizadaMinutos { get; set; }

        public bool Ativo { get; set; }
    }
}
