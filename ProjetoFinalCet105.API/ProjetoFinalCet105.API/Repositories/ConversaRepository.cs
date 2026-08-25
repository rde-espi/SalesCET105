using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class ConversaRepository : GenericRepository<Conversa>,IConversaRepository  
    {
        public ConversaRepository(DataContext context)
            : base(context)
        {
        }

        public IQueryable<Conversa> GetAllWithDetails()
        {
            return _context.Conversas
                .Include(c => c.Cliente)
                .Include(c => c.Funcionario)
                .Include(c => c.Mensagens)
                    .ThenInclude(m => m.Remetente)
                .AsNoTracking();
        }

        public async Task<Conversa?> GetByIdWithDetailsAsync(
            int id)
        {
            return await _context.Conversas
                .Include(c => c.Cliente)
                .Include(c => c.Funcionario)
                .Include(c => c.Mensagens)
                    .ThenInclude(m => m.Remetente)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Conversa?> GetConversaEntreUtilizadoresAsync(
            string clienteId,
            string funcionarioUserId)
        {
            return await _context.Conversas
                .FirstOrDefaultAsync(c =>
                    c.ClienteId == clienteId &&
                    c.FuncionarioUserId == funcionarioUserId);
        }
    }
}
