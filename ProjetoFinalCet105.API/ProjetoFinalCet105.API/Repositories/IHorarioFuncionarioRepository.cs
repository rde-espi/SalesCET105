using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IHorarioFuncionarioRepository:IGenericRepository<HorarioFuncionario>
    {
        IQueryable<HorarioFuncionario> GetAllWithFuncionario();

        Task<HorarioFuncionario?> GetByIdWithFuncionarioAsync(int id);

        Task<bool> ExisteSobreposicaoAsync(
    int funcionarioId,
    DayOfWeek diaSemana,
    TimeSpan horaInicio,
    TimeSpan horaFim,
    int? horarioIdIgnorar = null);
    }
}
