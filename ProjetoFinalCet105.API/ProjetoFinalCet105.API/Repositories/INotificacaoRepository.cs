using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface INotificacaoRepository:IGenericRepository<Notificacao>
    {
        IQueryable<Notificacao> GetByUserId(string userId);

        Task<Notificacao?> GetByIdAndUserIdAsync(int id,string userId);
        Task<List<Notificacao>> GetNaoLidasByUserIdAsync(string userId);
    }
}
