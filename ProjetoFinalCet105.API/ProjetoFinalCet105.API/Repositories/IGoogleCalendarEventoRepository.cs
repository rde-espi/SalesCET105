using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IGoogleCalendarEventoRepository : IGenericRepository<GoogleCalendarEvento>
    {
        Task<GoogleCalendarEvento?> GetByMarcacaoAndUserAsync(int marcacaoId, string userId);

        Task<List<GoogleCalendarEvento>> GetByMarcacaoIdAsync(int marcacaoId);
    }
}
