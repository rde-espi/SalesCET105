using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Services.GoogleCalendarService
{
    public interface IGoogleCalendarSyncService
    {
        Task SincronizarCriacaoMarcacaoAsync(Marcacao marcacao,CancellationToken cancellationToken = default);

        Task SincronizarAtualizacaoMarcacaoAsync(Marcacao marcacao, CancellationToken cancellationToken = default);

        Task SincronizarCancelamentoMarcacaoAsync( Marcacao marcacao, CancellationToken cancellationToken = default);
    }
}
