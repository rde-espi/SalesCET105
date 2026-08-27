using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IGoogleCalendarContaRepository : IGenericRepository<GoogleCalendarConta>
    {
        Task<GoogleCalendarConta?> GetByUserIdAsync(string userId);
    }
}
