using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class PermissaoAdminTemporariaRepository : GenericRepository<PermissaoAdminTemporaria>, IPermissaoAdminTemporariaRepository
    {
        public PermissaoAdminTemporariaRepository( DataContext context) : base(context)
        {
        }

        public IQueryable<PermissaoAdminTemporaria> GetAllWithUsers()
        {
            return _context.PermissoesAdminTemporarias
                .Include(p => p.FuncionarioUser)
                .Include(p => p.ConcedidoPorUser)
                .AsNoTracking();
        }
    }
}
