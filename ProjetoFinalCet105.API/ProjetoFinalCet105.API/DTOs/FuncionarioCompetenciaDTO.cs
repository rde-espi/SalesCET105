namespace ProjetoFinalCet105.API.DTOs
{
    public class FuncionarioCompetenciaDTO
    {
        public int Id { get; set; }

        public int FuncionarioId { get; set; }
        public string FuncionarioNome { get; set; } = string.Empty;

        public int CompetenciaId { get; set; }
        public string CompetenciaNome { get; set; } = string.Empty;

        public string? Nivel { get; set; }
        public string? Certificacao { get; set; }
    }
}
