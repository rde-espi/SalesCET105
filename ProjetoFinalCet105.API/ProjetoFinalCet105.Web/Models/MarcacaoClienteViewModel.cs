namespace ProjetoFinalCet105.Web.Models;

public class MarcacaoClienteViewModel
{
    public int Id { get; set; }

    public string ClienteId { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;

    public int FuncionarioId { get; set; }
    public string FuncionarioNome { get; set; } = string.Empty;

    public int ServicoId { get; set; }
    public string ServicoNome { get; set; } = string.Empty;

    public int EstadoMarcacaoId { get; set; }
    public string EstadoMarcacaoNome { get; set; } = string.Empty;

    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }

    public decimal Preco { get; set; }

    public string? Observacoes { get; set; }

    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    public int? PromoCodeId { get; set; }
    public string? PromoCode { get; set; }

    public decimal? PercentagemDescontoAplicada { get; set; }
    public decimal? ValorDesconto { get; set; }
}