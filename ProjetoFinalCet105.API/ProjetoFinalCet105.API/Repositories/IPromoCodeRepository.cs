using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IPromoCodeRepository:IGenericRepository<PromoCode>
    {
        Task<PromoCode?> GetByCodigoAsync(string codigo);
        Task<bool> ClienteJaUsouPromoCodeAsync( string clienteId, int promoCodeId);
        Task IncrementarUtilizacaoAsync(int promoCodeId);
        Task<PromoCode?> GetByIdTrackedAsync(int id);
    }
}
