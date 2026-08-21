using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class FeedbackRepository:GenericRepository<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(DataContext context):base(context)
        {
            
        }

        public async Task<bool> ExisteFeedbackMarcacaoAsync(int marcacaoId)
        {
            return await _context.Feedbacks
            .AnyAsync(f => f.MarcacaoId == marcacaoId);
        }

        public IQueryable<Feedback> GetAllWithDetails()
        {
            return _context.Feedbacks
            .Include(f => f.Cliente)
            .Include(f => f.Funcionario)
                .ThenInclude(funcionario => funcionario.User)
            .Include(f => f.Marcacao)
            .AsNoTracking();
        }

        public async Task<Feedback?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Feedbacks
            .Include(f => f.Cliente)
            .Include(f => f.Funcionario)
                .ThenInclude(funcionario => funcionario.User)
            .Include(f => f.Marcacao)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id);
        }
    }
}
