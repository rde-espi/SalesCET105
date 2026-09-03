namespace ProjetoFinalCet105.API.Services.DashboardService
{
    public interface IOcupacaoAgendaService
    {
        Task<(decimal HorasDisponiveis, decimal HorasOcupadas)>CalcularAsync( DateTime dataInicio,DateTime dataFim);
    }
}
