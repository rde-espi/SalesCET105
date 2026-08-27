using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class GoogleCalendarEventoRepository : GenericRepository<GoogleCalendarEvento>, IGoogleCalendarEventoRepository
    {
        public GoogleCalendarEventoRepository(DataContext context) : base(context)
        {
        }

        public async Task<GoogleCalendarEvento?> GetByMarcacaoAndUserAsync(int marcacaoId, string userId)
        {
            return await _context.GoogleCalendarEventos
                .AsNoTracking()
                .FirstOrDefaultAsync(g =>
                g.MarcacaoId == marcacaoId &&
                g.UserId == userId);
        }

        public async Task<List<GoogleCalendarEvento>> GetByMarcacaoIdAsync( int marcacaoId)
        {
            return await _context.GoogleCalendarEventos
                .AsNoTracking()
                .Where(g => g.MarcacaoId == marcacaoId)
                .ToListAsync();
        }
    }
}
