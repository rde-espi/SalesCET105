using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class PromoCodeRepository:GenericRepository<PromoCode>,IPromoCodeRepository
    {
        public PromoCodeRepository(DataContext context): base(context)
        {
            
        }
    }
}
