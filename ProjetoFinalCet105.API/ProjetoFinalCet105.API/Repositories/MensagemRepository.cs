using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class MensagemRepository : GenericRepository<Mensagem>, IMensagemRepository
    {
        public MensagemRepository(DataContext context): base(context)
        {
        }

        public IQueryable<Mensagem> GetByConversaId(int conversaId)
        {
            return _context.Mensagens
                .Where(m => m.ConversaId == conversaId)
                .Include(m => m.Remetente)
                .OrderBy(m => m.DataEnvio)
                .AsNoTracking();
        }

        public async Task<List<Mensagem>> GetNaoLidasAsync(int conversaId, string userId)
        {
            return await _context.Mensagens
                .Where(m =>
                    m.ConversaId == conversaId &&
                    m.RemetenteId != userId &&
                    !m.Lida)
                .ToListAsync();
        }

        public async Task<int> CountNaoLidasByUserIdAsync(string userId)
        {
            return await _context.Mensagens
                .Where(m =>
                    !m.Lida &&
                    m.RemetenteId != userId &&
                    (
                        m.Conversa.ClienteId == userId ||
                        m.Conversa.FuncionarioUserId == userId
                    ))
                .CountAsync();
        }
    }
}
