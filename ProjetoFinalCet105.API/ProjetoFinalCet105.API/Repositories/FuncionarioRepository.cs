using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class FuncionarioRepository:GenericRepository<Funcionario>,IFuncionarioRepository
    {
        public FuncionarioRepository(DataContext context) :base(context)
        {
            
        }

        public IQueryable<Funcionario> GetAllFuncionariosWithUser()
        {
            return _context.Funcionarios
                .Include(f => f.User)
                .AsNoTracking();
        }

        public async Task<Funcionario?> GetFuncionarioByIdAsync(int id)
        {
            return await _context.Funcionarios
                .Include(f => f.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Funcionario?> GetFuncionarioByUserIdAsync(string userId)
        {
            return await _context.Funcionarios
                .Include(f => f.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.UserId == userId);
        }
    }
}
