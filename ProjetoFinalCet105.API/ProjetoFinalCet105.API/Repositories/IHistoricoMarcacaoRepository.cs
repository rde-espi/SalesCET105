using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IHistoricoMarcacaoRepository:IGenericRepository<HistoricoMarcacao>
    {
        IQueryable<HistoricoMarcacao> GetAllWithDetails();
        Task<HistoricoMarcacao?> GetByIdWithDetails(int id);
    }
}
