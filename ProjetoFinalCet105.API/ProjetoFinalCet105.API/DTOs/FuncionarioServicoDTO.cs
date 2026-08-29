using System.ComponentModel.DataAnnotations;

namespace ProjetoFinalCet105.API.DTOs
{
    public class FuncionarioServicoDTO
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O funcionário indicado é inválido.")]
        public int FuncionarioId { get; set; }

        public string FuncionarioNome { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "O serviço indicado é inválido.")]
        public int ServicoId { get; set; }

        public string ServicoNome { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "9999999999999999",
            ErrorMessage = "O preço personalizado não pode ser negativo.")]
        public decimal? PrecoPersonalizado { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "A duração personalizada deve ser superior a zero.")]
        public int? DuracaoPersonalizadaMinutos { get; set; }

        public bool Ativo { get; set; }
    }
}
