namespace ProjetoFinalCet105.Web.Models
{
    public class DashboardViewModel
    {
        public DashboardFinanceiroViewModel? Financeiro { get; set; }

        public DashboardAgendaViewModel? Agenda { get; set; }

        public DashboardClientesViewModel? Clientes { get; set; }

        public List<FaturacaoMensalViewModel> EvolucaoMensal { get; set; } = new();

        public List<ServicoFaturacaoViewModel> ServicosMaisFaturados { get; set; } = new();

        public List<FaturacaoCategoriaViewModel> FaturacaoPorCategoria { get; set; } = new();

        public List<DesempenhoFuncionarioViewModel> Equipa { get; set; } = new();
        public List<MarcacaoDashboardViewModel> MarcacoesHoje { get; set; } = new();
        public List<ClienteRecenteViewModel> UltimosClientes { get; set; } = new();
        public List<NotificacaoDashboardViewModel> NotificacoesRecentes { get; set; } = new();
        public List<FaturacaoMensalViewModel> EvolucaoMensalCompleta { get; set; } = new();
        public List<ServicoMaisMarcadoViewModel> ServicosMaisMarcados { get; set; } = new();

        public List<HorarioMaiorProcuraViewModel> HorariosMaiorProcura { get; set; } = new();

        public List<DiaSemanaProcuraViewModel> DiasMaiorProcura { get; set; } = new();

        public int NotificacoesNaoLidas { get; set; }
    }


    public class DashboardFinanceiroViewModel
    {
        public decimal FaturacaoHoje { get; set; }

        public decimal FaturacaoSemana { get; set; }

        public decimal FaturacaoMes { get; set; }

        public int TotalFaturasMes { get; set; }

        public decimal TicketMedioMes { get; set; }

        public int FaturasComNifMes { get; set; }

        public int FaturasSemNifMes { get; set; }

        public decimal ValorComNifMes { get; set; }

        public decimal ValorSemNifMes { get; set; }

        public decimal DespesasMes { get; set; }

        public decimal ResultadoMes { get; set; }

        public decimal MargemPercentualMes { get; set; }
    }


    public class DashboardAgendaViewModel
    {
        public int TotalMarcacoesMes { get; set; }

        public int MarcacoesConcluidasMes { get; set; }

        public int MarcacoesCanceladasMes { get; set; }

        public int NaoCompareceuMes { get; set; }

        public int MarcacoesPendentesMes { get; set; }

        public int MarcacoesConfirmadasMes { get; set; }

        public decimal TaxaConclusao { get; set; }

        public decimal TaxaCancelamento { get; set; }

        public decimal TaxaNaoComparecimento { get; set; }

        public decimal HorasDisponiveisMes { get; set; }

        public decimal HorasOcupadasMes { get; set; }

        public decimal TaxaOcupacao { get; set; }

        public decimal HorasLivresMes { get; set; }
    }


    public class DashboardClientesViewModel
    {
        public int TotalClientes { get; set; }

        public int NovosClientesMes { get; set; }

        public int ClientesRecorrentes { get; set; }

        public int ClientesInativos60Dias { get; set; }

        public int ClientesInativos90Dias { get; set; }

        public decimal TaxaRecorrencia { get; set; }
    }


    public class FaturacaoMensalViewModel
    {
        public int Ano { get; set; }

        public int Mes { get; set; }

        public string NomeMes { get; set; } = string.Empty;

        public decimal Total { get; set; }
    }


    public class ServicoFaturacaoViewModel
    {
        public int? ServicoId { get; set; }

        public string NomeServico { get; set; } = string.Empty;

        public int Quantidade { get; set; }

        public decimal TotalFaturado { get; set; }
    }


    public class FaturacaoCategoriaViewModel
    {
        public int CategoriaId { get; set; }

        public string NomeCategoria { get; set; } = string.Empty;

        public int QuantidadeServicos { get; set; }

        public decimal TotalFaturado { get; set; }
    }


    public class DesempenhoFuncionarioViewModel
    {
        public int FuncionarioId { get; set; }

        public string NomeFuncionario { get; set; } = string.Empty;

        public int MarcacoesConcluidas { get; set; }

        public decimal TotalFaturado { get; set; }

        public decimal AvaliacaoMedia { get; set; }
    }

    public class MarcacaoDashboardViewModel
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
    }

    public class ClienteRecenteViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string NomeCompleto { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? FotografiaUrl { get; set; }

        public bool Ativo { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime? DataAtualizacao { get; set; }
    }
    public class NotificacaoDashboardViewModel
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Mensagem { get; set; } = string.Empty;

        public bool Lida { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime? DataLeitura { get; set; }
    }

    public class ServicoMaisMarcadoViewModel
    {
        public int ServicoId { get; set; }
        public string NomeServico { get; set; } = string.Empty;
        public int QuantidadeMarcacoes { get; set; }
        public decimal Percentagem { get; set; }
    }

    public class HorarioMaiorProcuraViewModel
    {
        public int Hora { get; set; }
        public string FaixaHoraria { get; set; } = string.Empty;
        public int QuantidadeMarcacoes { get; set; }
        public decimal Percentagem { get; set; }
    }

    public class DiaSemanaProcuraViewModel
    {
        public int DiaSemana { get; set; }
        public string NomeDia { get; set; } = string.Empty;
        public int QuantidadeMarcacoes { get; set; }
        public decimal Percentagem { get; set; }
    }
}
