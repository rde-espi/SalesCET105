using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class FaturaRepository : GenericRepository<Fatura>, IFaturaRepository
    {
        public FaturaRepository(DataContext context) : base(context)
        {
        }

        public async Task<Fatura?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Faturas
                .AsNoTracking()
                .Include(f => f.Itens)
                .Include(f => f.Marcacao)
                    .ThenInclude(m => m.Servico)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Fatura?> GetByMarcacaoIdAsync(int marcacaoId)
        {
            return await _context.Faturas
                .AsNoTracking()
                .Include(f => f.Itens)
                .FirstOrDefaultAsync(f => f.MarcacaoId == marcacaoId);
        }

        public IQueryable<Fatura> GetAllWithDetails()
        {
            return _context.Faturas
                .AsNoTracking()
                .Include(f => f.Itens)
                .Include(f => f.Marcacao)
                    .ThenInclude(m => m.Servico);
        }

        public async Task<int> GetProximoNumeroSequencialAsync(string serie)
        {
            var ultimoNumero = await _context.Faturas
                .Where(f => f.Serie == serie)
                .MaxAsync(f => (int?)f.NumeroSequencial);

            return (ultimoNumero ?? 0) + 1;
        }
    }
}
