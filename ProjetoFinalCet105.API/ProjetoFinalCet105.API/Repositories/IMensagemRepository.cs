using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IMensagemRepository:IGenericRepository<Mensagem>
    {
        IQueryable<Mensagem> GetByConversaId(int conversaId);

        Task<List<Mensagem>> GetNaoLidasAsync(
            int conversaId,
            string userId);

        Task<int> CountNaoLidasByUserIdAsync(string userId);
    }
}
