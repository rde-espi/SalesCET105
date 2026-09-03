using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class DespesaRepository:GenericRepository<Despesa>,IDespesaRepository
    {
        public DespesaRepository(DataContext context) : base(context)
        {
            
        }
        public IQueryable<Despesa> GetAllDespesas()
        {
            return _context.Despesas
                .AsNoTracking();
        }
    }
}
