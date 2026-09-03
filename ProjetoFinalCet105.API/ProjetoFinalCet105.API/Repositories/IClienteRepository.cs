using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IClienteRepository
    {
        Task<IList<User>> GetAllClientesAsync();
    }
}
