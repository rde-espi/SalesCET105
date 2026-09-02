namespace ProjetoFinalCet105.API.DTOs
{
    public class ResultadoValidacaoNifDTO
    {
        public string Nif { get; set; } = string.Empty;

        public bool FormatoValido { get; set; }

        public bool VerificadoExternamente { get; set; }

        public bool EncontradoExternamente { get; set; }

        public string? Nome { get; set; }

        public string? Estado { get; set; }
    }
}
