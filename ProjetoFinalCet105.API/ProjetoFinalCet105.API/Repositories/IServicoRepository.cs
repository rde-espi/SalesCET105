using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IServicoRepository:IGenericRepository<Servico>
    {
        IQueryable<Servico> GetAllWithCategoria();
        Task<Servico?>GetByIdWithCategoriaAsync(int id);
    }
}
