using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class FuncionarioServicoRepository:GenericRepository<FuncionarioServico>,IFuncionarioServicoRepository
    {
        public FuncionarioServicoRepository(DataContext context): base(context)
        {
            
        }

        public IQueryable<FuncionarioServico> GetAllWithDetails()
        {
            return _context.FuncionariosServicos
                .Include(fs => fs.Funcionario)
                .ThenInclude(f => f.User)
                .Include(fs => fs.Servico)
                .AsNoTracking();
        }

        public Task<FuncionarioServico?> GetByIdWithDetailsAsync(int id)
        {
            return _context.FuncionariosServicos
                .Include(fs => fs.Funcionario)
                .ThenInclude(f => f.User)
                .Include(fs => fs.Servico)
                .AsNoTracking()
                .FirstOrDefaultAsync(fs => fs.Id == id);
        }
        public async Task<bool> ExistFuncionarioServicoAsync(int funcionarioId,int servicoId)
        {
            return await _context.FuncionariosServicos
                .AnyAsync(fs =>
                    fs.FuncionarioId == funcionarioId &&
                    fs.ServicoId == servicoId);
        }
    }
}
