using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class DispositivoUserRepository : GenericRepository<DispositivoUser>,IDispositivoUserRepository
    {
        public DispositivoUserRepository(
            DataContext context)
            : base(context)
        {
        }

        public async Task<DispositivoUser?> GetByFidAsync(string fid)
        {
            return await _context.DispositivosUsers
                .FirstOrDefaultAsync(d => d.Fid == fid);
        }

        public IQueryable<DispositivoUser> GetAtivosByUserId(string userId)
        {
            return _context.DispositivosUsers
                .Where(d =>
                    d.UserId == userId &&
                    d.Ativo)
                .AsNoTracking();
        }
    }
}
