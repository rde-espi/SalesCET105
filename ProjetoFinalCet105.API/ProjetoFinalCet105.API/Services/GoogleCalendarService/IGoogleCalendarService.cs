using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Services.GoogleCalendarService
{
    public interface IGoogleCalendarService
    {
        string GerarUrlAutorizacao(string userId);
        string? ObterUserIdDoState(string state);

        Task<GoogleCalendarTokenDTO?> TrocarCodigoPorRefreshTokenAsync( string code, CancellationToken cancellationToken = default);
        Task<string> CriarEventoAsync(
            GoogleCalendarConta conta,
            string titulo,
            string descricao,
            DateTime inicio,
            DateTime fim,
            CancellationToken cancellationToken = default);

        Task AtualizarEventoAsync(
            GoogleCalendarConta conta,
            string googleEventId,
            string titulo,
            string descricao,
            DateTime inicio,
            DateTime fim,
            CancellationToken cancellationToken = default);

        Task EliminarEventoAsync(
            GoogleCalendarConta conta,
            string googleEventId,
            CancellationToken cancellationToken = default);
    }
}
