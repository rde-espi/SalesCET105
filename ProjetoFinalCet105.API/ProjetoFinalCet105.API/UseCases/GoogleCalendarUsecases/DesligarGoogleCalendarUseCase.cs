using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.UseCases.GoogleCalendarUsecases
{
    public class DesligarGoogleCalendarUseCase
    {
        private readonly IGoogleCalendarContaRepository
            _googleCalendarContaRepository;

        public DesligarGoogleCalendarUseCase(IGoogleCalendarContaRepository googleCalendarContaRepository)
        {
            _googleCalendarContaRepository = googleCalendarContaRepository;
        }

        public async Task<bool> ExecuteAsync(string userId)
        {
            var conta = await _googleCalendarContaRepository.GetByUserIdAsync(userId);

            if (conta == null || !conta.Ativo)
            {
                return false;
            }

            conta.Ativo = false;
            conta.DataAtualizacao = DateTime.UtcNow;

            await _googleCalendarContaRepository.UpdateAsync(conta);

            return true;
        }
    }
}
