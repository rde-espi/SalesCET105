using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class HistoricoMarcacaoRepository : GenericRepository<HistoricoMarcacao>, IHistoricoMarcacaoRepository
    {
        public HistoricoMarcacaoRepository(DataContext context):base(context)
        {
            
        }

        public IQueryable<HistoricoMarcacao> GetAllWithDetails()
        {
            return _context.HistoricosMarcacoes
                .Include(h => h.Marcacao)
                .Include(h => h.User)
                .AsNoTracking();
        }

        public async Task<HistoricoMarcacao?> GetByIdWithDetails(int id)
        {
            return await _context.HistoricosMarcacoes
                .Include(h => h.Marcacao)
                .Include(h => h.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id);
        }
    }
}
