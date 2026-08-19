using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class MarcacaoRepository:GenericRepository<Marcacao>, IMarcacaoRepository
    {
        public MarcacaoRepository(DataContext context):base(context)
        {
            
        }

        public async Task<bool> ExisteSobreposicaoAsync(int funcionarioId, DateTime dataHoraInicio, DateTime dataHoraFim, int? marcacaoIdIgnorar = null)
        {
            return await _context.Marcacoes.AnyAsync
                (m =>m.FuncionarioId == funcionarioId &&
           m.EstadoMarcacao.Nome != "Cancelada" &&
           (!marcacaoIdIgnorar.HasValue ||  m.Id != marcacaoIdIgnorar.Value) &&
           dataHoraInicio < m.DataHoraFim &&
           dataHoraFim > m.DataHoraInicio);
        }

        public IQueryable<Marcacao> GetAllWithDetails()
        {
            return _context.Marcacoes
            .Include(m => m.Cliente)
            .Include(m => m.Funcionario)
                .ThenInclude(f => f.User)
            .Include(m => m.Servico)
            .Include(m => m.EstadoMarcacao)
            .AsNoTracking();
        }

        public async Task<Marcacao?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Marcacoes
            .Include(m => m.Cliente)
            .Include(m => m.Funcionario)
                .ThenInclude(f => f.User)
            .Include(m => m.Servico)
            .Include(m => m.EstadoMarcacao)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
