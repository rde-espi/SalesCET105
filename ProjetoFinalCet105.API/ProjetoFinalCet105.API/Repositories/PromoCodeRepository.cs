using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class PromoCodeRepository:GenericRepository<PromoCode>,IPromoCodeRepository
    {
        public PromoCodeRepository(DataContext context): base(context)
        {
            
        }
        public async Task<PromoCode?> GetByCodigoAsync(string codigo)
        {
            return await _context.PromoCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                p.Codigo == codigo);
        }

        public async Task<bool> ClienteJaUsouPromoCodeAsync(string clienteId,int promoCodeId)
        {
            return await _context.Marcacoes
                .AnyAsync(m =>
                    m.ClienteId == clienteId &&
                    m.PromoCodeId == promoCodeId);
        }

        public async Task IncrementarUtilizacaoAsync(int promoCodeId)
        {
            var promoCode = await _context.PromoCodes.FirstOrDefaultAsync(p => p.Id == promoCodeId);

            if (promoCode == null)
            {
                return;
            }

            promoCode.NumeroUtilizacoes++;

            await _context.SaveChangesAsync();
        }

        public async Task<PromoCode?> GetByIdTrackedAsync(int id)
        {
            return await _context.PromoCodes
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
