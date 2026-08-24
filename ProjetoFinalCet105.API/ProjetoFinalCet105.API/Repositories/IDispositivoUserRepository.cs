using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IDispositivoUserRepository :IGenericRepository<DispositivoUser>
    {
        Task<DispositivoUser?> GetByFidAsync(string fid);

        IQueryable<DispositivoUser> GetAtivosByUserId(string userId);
    }
}
