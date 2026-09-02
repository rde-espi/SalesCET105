using Microsoft.Extensions.Options;
using ProjetoFinalCet105.API.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace ProjetoFinalCet105.API.Services.Faturacao
{
    public class FaturaPdfService : IFaturaPdfService
    {
        private const string CorDourada = "#A87418";
        private const string CorDouradaClara = "#D7B978";
        private const string CorBege = "#F5F1E8";
        private const string CorCinzaClaro = "#F7F7F7";
        private const string CorTexto = "#171717";
        private const string CorLinha = "#D3D3D3";
        private readonly FaturacaoSettings _settings;
        private readonly CultureInfo _culture =
            CultureInfo.GetCultureInfo("pt-PT");
        private readonly IWebHostEnvironment _environment;

        public FaturaPdfService(IOptions<FaturacaoSettings> options, IWebHostEnvironment environment)
        {
            _settings = options.Value;
            _environment = environment;
        }

        public byte[] GerarPdf(FaturaDTO fatura)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);

                    page.DefaultTextStyle(
                        style => style.FontSize(10));

                    page.Header()
                        .Element(header =>
                            CriarCabecalho(header, fatura));

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            column.Item()
                                .Element(container =>
                                    CriarDadosCliente(
                                        container,
                                        fatura));

                            column.Item()
    .Element(container =>
        CriarTabela(
            container,
            fatura));

                            column.Item()
    .Element(container =>
        CriarTotais(
            container,
            fatura));
                        });

                    page.Footer()
    .Element(footer =>
        CriarRodape(footer, fatura));
                });
            });

            return document.GeneratePdf();
        }

        private void CriarCabecalho(
    IContainer container,
    FaturaDTO fatura)
        {
            var logoPath = Path.Combine(
                _environment.ContentRootPath,
                _settings.LogoPath);

            container
                .Height(105)
                .Row(row =>
                {
                    // LOGÓTIPO
                    row.ConstantItem(140)
                        .PaddingRight(12)
                        .Element(logoContainer =>
                        {
                            if (File.Exists(logoPath))
                            {
                                logoContainer
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Image(logoPath)
                                    .FitArea();
                            }
                            else
                            {
                                logoContainer
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Text(_settings.EmitenteNome)
                                    .Bold();
                            }
                        });

                    // DADOS DO EMITENTE
                    row.RelativeItem()
                        .PaddingTop(8)
                        .PaddingRight(15)
                        .Column(column =>
                        {
                            column.Spacing(4);

                            column.Item()
                                .Text(_settings.EmitenteNome)
                                .FontSize(15)
                                .Bold()
                                .FontColor(CorTexto);

                            column.Item()
                                .Text($"NIF: {_settings.EmitenteNif}")
                                .FontSize(9);

                            column.Item()
                                .Text(_settings.EmitenteMorada)
                                .FontSize(9);

                            column.Item()
                                .Text(
                                    $"{_settings.EmitenteCodigoPostal} " +
                                    $"{_settings.EmitenteLocalidade}")
                                .FontSize(9);
                        });

                    // SEPARADOR DOURADO
                    row.ConstantItem(2)
                        .Background(CorDourada);

                    // FATURA
                    row.ConstantItem(165)
                        .PaddingLeft(18)
                        .PaddingTop(5)
                        .Column(column =>
                        {
                            column.Spacing(5);

                            column.Item()
                                .AlignRight()
                                .Text("FATURA")
                                .FontSize(23)
                                .Bold()
                                .FontColor(CorTexto);

                            column.Item()
                                .AlignRight()
                                .Text(fatura.Numero)
                                .FontSize(14)
                                .Bold()
                                .FontColor(CorDourada);

                            column.Item()
                                .PaddingTop(5)
                                .Height(1)
                                .Background(CorDourada);

                            column.Item()
                                .PaddingTop(7)
                                .AlignRight()
                                .Text(
                                    $"Data: {fatura.DataEmissao.ToString("dd/MM/yyyy", _culture)}")
                                .FontSize(10)
                                .Bold();
                        });
                });
        }

        private void CriarDadosCliente(
    IContainer container,
    FaturaDTO fatura)
        {
            container.Row(row =>
            {
                // =========================
                // DADOS DO CLIENTE
                // =========================
                row.RelativeItem(2)
                    .PaddingRight(20)
                    .Column(column =>
                    {
                        column.Spacing(7);

                        column.Item()
                            .Background(CorBege)
                            .PaddingVertical(7)
                            .PaddingHorizontal(12)
                            .Text("CLIENTE")
                            .FontSize(12)
                            .Bold()
                            .FontColor(CorTexto);

                        column.Item()
                            .PaddingTop(5)
                            .PaddingHorizontal(12)
                            .Row(r =>
                            {
                                r.ConstantItem(95)
                                    .Text("Nome:")
                                    .Bold();

                                r.RelativeItem()
                                    .Text(fatura.NomeCliente ?? "-");
                            });

                        if (!string.IsNullOrWhiteSpace(fatura.NifCliente))
                        {
                            column.Item()
                                .PaddingHorizontal(12)
                                .Row(r =>
                                {
                                    r.ConstantItem(95)
                                        .Text("NIF:")
                                        .Bold();

                                    r.RelativeItem()
                                        .Text(fatura.NifCliente);
                                });
                        }

                        if (!string.IsNullOrWhiteSpace(fatura.MoradaCliente))
                        {
                            column.Item()
                                .PaddingHorizontal(12)
                                .Row(r =>
                                {
                                    r.ConstantItem(95)
                                        .Text("Morada:")
                                        .Bold();

                                    r.RelativeItem()
                                        .Text(fatura.MoradaCliente);
                                });
                        }

                        if (!string.IsNullOrWhiteSpace(fatura.CodigoPostalCliente))
                        {
                            column.Item()
                                .PaddingHorizontal(12)
                                .Row(r =>
                                {
                                    r.ConstantItem(95)
                                        .Text("Código Postal:")
                                        .Bold();

                                    r.RelativeItem()
                                        .Text(fatura.CodigoPostalCliente);
                                });
                        }

                        if (!string.IsNullOrWhiteSpace(fatura.LocalidadeCliente))
                        {
                            column.Item()
                                .PaddingHorizontal(12)
                                .Row(r =>
                                {
                                    r.ConstantItem(95)
                                        .Text("Localidade:")
                                        .Bold();

                                    r.RelativeItem()
                                        .Text(fatura.LocalidadeCliente);
                                });
                        }
                    });

                // =========================
                // BLOCO DA MARCAÇÃO
                // =========================
                row.RelativeItem()
                    .PaddingTop(45)
                    .Border(1)
                    .BorderColor(CorDouradaClara)
                    .Background(CorCinzaClaro)
                    .Padding(14)
                    .Column(column =>
                    {
                        column.Spacing(8);
               

                        column.Item()
                            .Text("MARCAÇÃO")
                            .FontSize(10)
                            .Bold()
                            .FontColor(CorDourada);

                        column.Item()
                            .Height(1)
                            .Background(CorDouradaClara);

                        column.Item()
                            .PaddingTop(3)
                            .Row(r =>
                            {
                                r.ConstantItem(95)
                                    .Text("Marcação n.º:")
                                    .Bold();

                                r.RelativeItem()
                                    .AlignRight()
                                    .Text(fatura.MarcacaoId.ToString());
                            });

                        column.Item()
    .Row(r =>
    {
        r.RelativeItem()
            .Text("Data:")
            .Bold();

        r.ConstantItem(95)
            .AlignRight()
            .Text(
                fatura.DataMarcacao.ToString(
                    "dd/MM/yyyy",
                    _culture));
    });

                        column.Item()
                            .Row(r =>
                            {
                                r.ConstantItem(110)
                                    .Text("Hora:")
                                    .Bold();

                                r.RelativeItem()
                                    .AlignRight()
                                    .Text(
                                        fatura.DataMarcacao.ToString(
                                            "HH:mm",
                                            _culture));
                            });
                    });
            });
        }
        private void CriarTabela(
    IContainer container,
    FaturaDTO fatura)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1.8f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.8f);
                });

                // CABEÇALHO
                table.Header(header =>
                {
                    header.Cell()
                        .Background(CorBege)
                        .PaddingVertical(8)
                        .PaddingHorizontal(10)
                        .Text("Descrição")
                        .Bold();

                    header.Cell()
                        .Background(CorBege)
                        .PaddingVertical(8)
                        .AlignCenter()
                        .Text("Qtd.")
                        .Bold();

                    header.Cell()
                        .Background(CorBege)
                        .PaddingVertical(8)
                        .AlignRight()
                        .Text("Preço Unitário")
                        .Bold();

                    header.Cell()
                        .Background(CorBege)
                        .PaddingVertical(8)
                        .AlignCenter()
                        .Text("IVA")
                        .Bold();

                    header.Cell()
                        .Background(CorBege)
                        .PaddingVertical(8)
                        .PaddingHorizontal(10)
                        .AlignRight()
                        .Text("Total")
                        .Bold();
                });

                // LINHAS
                foreach (var item in fatura.Itens)
                {
                    table.Cell()
                        .BorderBottom(1)
                        .BorderColor(CorLinha)
                        .PaddingVertical(9)
                        .PaddingHorizontal(10)
                        .Text(item.Descricao);

                    table.Cell()
                        .BorderBottom(1)
                        .BorderColor(CorLinha)
                        .PaddingVertical(9)
                        .AlignCenter()
                        .Text(item.Quantidade.ToString("0.##", _culture));

                    table.Cell()
                        .BorderBottom(1)
                        .BorderColor(CorLinha)
                        .PaddingVertical(9)
                        .AlignRight()
                        .Text(item.PrecoUnitario.ToString("C2", _culture));

                    table.Cell()
                        .BorderBottom(1)
                        .BorderColor(CorLinha)
                        .PaddingVertical(9)
                        .AlignCenter()
                        .Text($"{item.PercentagemIva:0.##}%");

                    table.Cell()
                        .BorderBottom(1)
                        .BorderColor(CorLinha)
                        .PaddingVertical(9)
                        .PaddingHorizontal(10)
                        .AlignRight()
                        .Text(item.Total.ToString("C2", _culture));
                }
            });
        }

        private void CriarTotais(
    IContainer container,
    FaturaDTO fatura)
        {
            container.Row(row =>
            {
                // Espaço vazio à esquerda
                row.RelativeItem();

                // Bloco dos totais à direita
                row.ConstantItem(300)
                    .Column(column =>
                    {
                        column.Spacing(0);

                        // SUBTOTAL
                        column.Item()
                            .BorderTop(1)
                            .BorderColor(CorLinha)
                            .PaddingVertical(8)
                            .PaddingHorizontal(12)
                            .Row(r =>
                            {
                                r.RelativeItem()
                                    .Text("Subtotal:")
                                    .Bold();

                                r.ConstantItem(100)
                                    .AlignRight()
                                    .Text(fatura.Subtotal.ToString("C2", _culture));
                            });

                        // DESCONTO - só aparece quando existe
                        if (fatura.ValorDesconto > 0)
                        {
                            column.Item()
                                .BorderTop(1)
                                .BorderColor(CorLinha)
                                .PaddingVertical(8)
                                .PaddingHorizontal(12)
                                .Row(r =>
                                {
                                    r.RelativeItem()
                                        .Text("Desconto:")
                                        .Bold();

                                    r.ConstantItem(100)
                                        .AlignRight()
                                        .Text(
                                            $"-{fatura.ValorDesconto.ToString("C2", _culture)}");
                                });
                        }

                        // IVA
                        column.Item()
                            .BorderTop(1)
                            .BorderColor(CorLinha)
                            .PaddingVertical(8)
                            .PaddingHorizontal(12)
                            .Row(r =>
                            {
                                r.RelativeItem()
                                    .Text("IVA incluído:")
                                    .Bold();

                                r.ConstantItem(100)
                                    .AlignRight()
                                    .Text(fatura.ValorIva.ToString("C2", _culture));
                            });

                        // TOTAL DESTACADO
                        column.Item()
                            .BorderTop(1.5f)
                            .BorderColor(CorDourada)
                            .Background(CorBege)
                            .PaddingVertical(10)
                            .PaddingHorizontal(12)
                            .Row(r =>
                            {
                                r.RelativeItem()
                                    .Text("TOTAL (IVA incluído):")
                                    .FontSize(13)
                                    .Bold()
                                    .FontColor(CorDourada);

                                r.ConstantItem(105)
                                    .AlignRight()
                                    .Text(fatura.Total.ToString("C2", _culture))
                                    .FontSize(15)
                                    .Bold()
                                    .FontColor(CorDourada);
                            });
                    });
            });
        }

        private void CriarLinhaTotal(
            ColumnDescriptor column,
            string descricao,
            decimal valor)
        {
            column.Item()
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text(descricao);

                    row.RelativeItem()
                        .AlignRight()
                        .Text(FormatarMoeda(valor));
                });
        }

        private static IContainer CelulaCabecalho(
            IContainer container)
        {
            return container
                .BorderBottom(1)
                .PaddingVertical(6)
                .PaddingHorizontal(3);
        }

        private static IContainer CelulaConteudo(
            IContainer container)
        {
            return container
                .BorderBottom(0.5f)
                .PaddingVertical(7)
                .PaddingHorizontal(3);
        }
        private void CriarRodape(
    IContainer container,
    FaturaDTO fatura)
        {
            container.Column(column =>
            {
                column.Spacing(7);

                column.Item()
                    .Height(1)
                    .Background(CorDourada);

                column.Item()
                    .PaddingTop(4)
                    .AlignCenter()
                    .Text($"Documento gerado eletronicamente pela {_settings.EmitenteNome}.")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);

                column.Item()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.DefaultTextStyle(
                            style => style
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1));

                        text.Span("Página ");

                        text.CurrentPageNumber()
                            .Bold()
                            .FontColor(CorDourada);

                        text.Span(" de ");

                        text.TotalPages()
                            .Bold()
                            .FontColor(CorDourada);
                    });
            });
        }
        private string FormatarMoeda(decimal valor)
        {
            return valor.ToString("C2", _culture);
        }
    }
}
