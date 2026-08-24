using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class NotificacaoRepository:GenericRepository<Notificacao>,INotificacaoRepository
    {
        public NotificacaoRepository(DataContext context):base(context)
        {
            
        }

        public IQueryable<Notificacao> GetByUserId(string userId)
        {
            return _context.Notificacoes
                .Where(n => n.UserId == userId)
                .AsNoTracking();
        }

        public async Task<Notificacao?> GetByIdAndUserIdAsync(int id,string userId)
        {
            return await _context.Notificacoes
                .FirstOrDefaultAsync(n =>
                    n.Id == id &&
                    n.UserId == userId);
        }
    }
}
