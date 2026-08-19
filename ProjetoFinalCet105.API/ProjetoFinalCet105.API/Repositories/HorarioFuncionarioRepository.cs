using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class HorarioFuncionarioRepository:GenericRepository<HorarioFuncionario>, IHorarioFuncionarioRepository
    {
        public HorarioFuncionarioRepository(DataContext context) :base(context)
        {
            
        }
        public IQueryable<HorarioFuncionario> GetAllWithFuncionario()
        {
            return _context.HorariosFuncionarios
                .Include(h => h.Funcionario)
                    .ThenInclude(f => f.User)
                .AsNoTracking();
        }

        public async Task<HorarioFuncionario?> GetByIdWithFuncionarioAsync(int id)
        {
            return await _context.HorariosFuncionarios
                .Include(h => h.Funcionario)
                    .ThenInclude(f => f.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == id);
        }
        public async Task<bool> ExisteSobreposicaoAsync(
    int funcionarioId,
    DayOfWeek diaSemana,
    TimeSpan horaInicio,
    TimeSpan horaFim,
    int? horarioIdIgnorar = null)
        {
            return await _context.HorariosFuncionarios
                .AnyAsync(h =>
                    h.FuncionarioId == funcionarioId &&
                    h.DiaSemana == diaSemana &&
                    h.Ativo &&
                    (!horarioIdIgnorar.HasValue || h.Id != horarioIdIgnorar.Value) &&
                    horaInicio < h.HoraFim &&
                    horaFim > h.HoraInicio);
        }
    }
}
