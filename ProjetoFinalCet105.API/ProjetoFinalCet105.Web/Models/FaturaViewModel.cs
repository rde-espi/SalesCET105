namespace ProjetoFinalCet105.Web.Models;

public class FaturaViewModel
{
    public int Id { get; set; }

    public int MarcacaoId { get; set; }
    public DateTime DataMarcacao { get; set; }

    public string Numero { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public int NumeroSequencial { get; set; }

    public DateTime DataEmissao { get; set; }

    public string? NomeCliente { get; set; }
    public string? NifCliente { get; set; }

    public string? MoradaCliente { get; set; }
    public string? CodigoPostalCliente { get; set; }
    public string? LocalidadeCliente { get; set; }

    public decimal Subtotal { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorIva { get; set; }
    public decimal Total { get; set; }

    public string Estado { get; set; } = string.Empty;

    public bool ComunicadaAT { get; set; }
    public DateTime? DataComunicacaoAT { get; set; }

    public string? CodigoRespostaAT { get; set; }
    public string? MensagemRespostaAT { get; set; }
    public List<FaturaItemViewModel> Itens { get; set; } = new();
}
public class FaturaItemViewModel
{
    public int Id { get; set; }
    public int? ServicoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal PercentagemIva { get; set; }
    public decimal ValorIva { get; set; }
    public decimal Total { get; set; }
    public string? CodigoIva { get; set; }
    public string? MotivoIsencaoIva { get; set; }
}