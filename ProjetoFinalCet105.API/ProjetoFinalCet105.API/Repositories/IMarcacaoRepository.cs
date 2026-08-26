using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IMarcacaoRepository:IGenericRepository<Marcacao>
    {
        IQueryable<Marcacao> GetAllWithDetails();

        Task<Marcacao?> GetByIdWithDetailsAsync(int id);

        Task<bool> ExisteSobreposicaoAsync(
            int funcionarioId,
            DateTime dataHoraInicio,
            DateTime dataHoraFim,
            int? marcacaoIdIgnorar = null);
        
        Task<bool> ClienteJaUsouPromoCodeAsync(string clienteId,int promoCodeId);
    }
}
