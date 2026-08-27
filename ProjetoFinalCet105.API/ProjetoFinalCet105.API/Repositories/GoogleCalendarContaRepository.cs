using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class GoogleCalendarContaRepository : GenericRepository<GoogleCalendarConta>,IGoogleCalendarContaRepository
    {
        public GoogleCalendarContaRepository(DataContext context) : base(context)
        {
            
        }

        public async Task<GoogleCalendarConta?> GetByUserIdAsync(
           string userId)
        {
            return await _context.GoogleCalendarContas
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.UserId == userId);
        }
    }
}
