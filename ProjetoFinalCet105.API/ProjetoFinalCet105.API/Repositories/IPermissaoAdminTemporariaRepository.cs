using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IPermissaoAdminTemporariaRepository : IGenericRepository<PermissaoAdminTemporaria>
    {
        IQueryable<PermissaoAdminTemporaria> GetAllWithUsers();
    }
}
