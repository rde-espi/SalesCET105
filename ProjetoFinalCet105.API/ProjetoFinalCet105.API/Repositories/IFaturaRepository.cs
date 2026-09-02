using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IFaturaRepository : IGenericRepository<Fatura>
    {
        Task<Fatura?> GetByIdWithDetailsAsync(int id);

        Task<Fatura?> GetByMarcacaoIdAsync(int marcacaoId);

        IQueryable<Fatura> GetAllWithDetails();

        Task<int> GetProximoNumeroSequencialAsync(string serie);
    }
}
