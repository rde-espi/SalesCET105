using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.GoogleCalendarService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.GoogleCalendarUsecases
{
    public class CallbackGoogleCalendarUseCase
    {
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly IGoogleCalendarContaRepository _repository;

        public CallbackGoogleCalendarUseCase(
            IGoogleCalendarService googleCalendarService,
            IGoogleCalendarContaRepository repository)
        {
            _googleCalendarService = googleCalendarService;
            _repository = repository;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(
            string userId,
            string code)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(code))
            {
                return UseCaseResult<bool>.Falha(
                    "Dados de autorização inválidos.",
                    TipoErro.Validacao);
            }

            var refreshToken =
                await _googleCalendarService
                    .TrocarCodigoPorRefreshTokenAsync(code);

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return UseCaseResult<bool>.Falha(
                    "Não foi possível obter autorização do Google Calendar.",
                    TipoErro.Validacao);
            }

            var conta = await _repository.GetByUserIdAsync(userId);

            if (conta == null)
            {
                conta = new GoogleCalendarConta
                {
                    UserId = userId,
                    RefreshToken = refreshToken,
                    CalendarId = "primary",
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow
                };

                await _repository.CreateAsync(conta);
            }
            else
            {
                conta.RefreshToken = refreshToken;
                conta.Ativo = true;
                conta.DataAtualizacao = DateTime.UtcNow;

                await _repository.UpdateAsync(conta);
            }

            return UseCaseResult<bool>.Ok(true);
        }
    }
}
