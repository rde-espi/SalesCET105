using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class FuncionarioCompetenciaRepository:GenericRepository<FuncionarioCompetencia>,IFuncionarioCompetenciaRepository
    {
        public FuncionarioCompetenciaRepository(DataContext context):base(context)
        {
            
        }

        public IQueryable<FuncionarioCompetencia> GetAllWithDetails()
        {
            return _context.FuncionariosCompetencias
            .Include(fc => fc.Funcionario)
                .ThenInclude(f => f.User)
            .Include(fc => fc.Competencia)
            .AsNoTracking();
        }

        public async Task<FuncionarioCompetencia?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.FuncionariosCompetencias
            .Include(fc => fc.Funcionario)
                .ThenInclude(f => f.User)
            .Include(fc => fc.Competencia)
            .AsNoTracking()
            .FirstOrDefaultAsync(fc => fc.Id == id);
        }
    }
}
