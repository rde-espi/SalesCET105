using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.UseCases.GoogleCalendarUsecases
{
    public class GetGoogleCalendarStatusUseCase
    {
        private readonly IGoogleCalendarContaRepository _googleCalendarContaRepository;

        public GetGoogleCalendarStatusUseCase( IGoogleCalendarContaRepository googleCalendarContaRepository)
        {
            _googleCalendarContaRepository = googleCalendarContaRepository;
        }

        public async Task<GoogleCalendarStatusDTO> ExecuteAsync(string userId)
        {
            var conta = await _googleCalendarContaRepository.GetByUserIdAsync(userId);

            if (conta == null || !conta.Ativo)
            {
                return new GoogleCalendarStatusDTO
                {
                    Ligado = false,
                    GoogleEmail = null
                };
            }

            return new GoogleCalendarStatusDTO
            {
                Ligado = true,
                GoogleEmail = conta.GoogleEmail
            };
        }
    }
}
