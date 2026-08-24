using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IFuncionarioCompetenciaRepository:IGenericRepository<FuncionarioCompetencia>
    {
        IQueryable<FuncionarioCompetencia> GetAllWithDetails();

        Task<FuncionarioCompetencia?> GetByIdWithDetailsAsync(int id);
        Task<bool> ExisteFuncionarioCompetenciaAsync(int funcionarioId,int competenciaId,int? ignorarId = null);
    }
}
