namespace ProjetoFinalCet105.API.DTOs
{
    public class FuncionarioDTOpublico
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; }
        public string? FotografiaUrl { get; set; }
        public string? Biografia { get; set; }
        public bool Disponivel { get; set; }
    }
}
