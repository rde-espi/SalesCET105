using ProjetoFinalCet105.API.Services.GoogleCalendarService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.GoogleCalendarUsecases
{
    public class ConectarGoogleCalendarUseCase
    {
        private readonly IGoogleCalendarService _googleCalendarService;

        public ConectarGoogleCalendarUseCase(IGoogleCalendarService googleCalendarService)
        {
            _googleCalendarService = googleCalendarService;
        }

        public UseCaseResult<string> Execute(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return UseCaseResult<string>.Falha( "Utilizador inválido.", TipoErro.NaoAutorizado);
            }

            var url = _googleCalendarService.GerarUrlAutorizacao(userId);

            return UseCaseResult<string>.Ok(url);
        }
    }
}
