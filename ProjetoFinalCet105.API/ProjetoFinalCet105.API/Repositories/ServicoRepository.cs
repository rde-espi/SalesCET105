using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class ServicoRepository:GenericRepository<Servico>, IServicoRepository
    {
        public ServicoRepository(DataContext context) : base(context)
        {
            
        }

        public IQueryable<Servico> GetAllWithCategoria()
        {
            return _context.Servicos
                .Include(s => s.Categoria)
                .AsNoTracking();
        }

        public async Task<Servico?> GetByIdWithCategoriaAsync(int id)
        {
            return await _context.Servicos
                .Include (s => s.Categoria)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
