using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IConversaRepository:IGenericRepository<Conversa>
    {
        IQueryable<Conversa> GetAllWithDetails();

        Task<Conversa?> GetByIdWithDetailsAsync(int id);

        Task<Conversa?> GetConversaEntreUtilizadoresAsync(
            string clienteId,
            string funcionarioUserId);
    }
}
