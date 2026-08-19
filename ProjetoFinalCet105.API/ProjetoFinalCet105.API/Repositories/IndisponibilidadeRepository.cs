using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class IndisponibilidadeRepository:GenericRepository<Indisponibilidade>,IIndisponibilidadeRepository
    {
        public IndisponibilidadeRepository(DataContext context):base(context)
        {
            
        }

        public async Task<bool> ExisteSobreposiçãoAsync(int funcionarioId, DateTime inicio, DateTime fim, int? idIgnorar = null)
        {
            return await _context.Indisponibilidades
                .AnyAsync(i => i.FuncionarioId == funcionarioId &&
                (!idIgnorar.HasValue || i.Id != idIgnorar.Value) &&
                inicio < i.DataHoraFim &&
                fim > i.DataHoraInicio);
        }

        public IQueryable<Indisponibilidade> GetAllIndisponibilidadesWithFuncionario()
        {
            return _context.Indisponibilidades
            .Include(i => i.Funcionario)
                .ThenInclude(f => f.User)
            .AsNoTracking();
        }

        public async Task<Indisponibilidade?> GetIndisponibilidadeWithFuncionarioByIdAsync(int id)
        {
            return await _context.Indisponibilidades
                .Include(i => i.Funcionario)
                .ThenInclude(f => f.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);
        }
    }
}
