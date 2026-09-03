using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IDespesaRepository:IGenericRepository<Despesa>
    {
        IQueryable<Despesa> GetAllDespesas();
    }
}
