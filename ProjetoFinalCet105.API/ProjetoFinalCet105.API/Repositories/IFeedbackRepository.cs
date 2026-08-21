using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IFeedbackRepository:IGenericRepository<Feedback>
    {
        IQueryable<Feedback> GetAllWithDetails();

        Task<Feedback?> GetByIdWithDetailsAsync(int id);

        Task<bool> ExisteFeedbackMarcacaoAsync(int marcacaoId);
    }
}
