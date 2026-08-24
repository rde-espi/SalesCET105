using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IFuncionarioServicoRepository:IGenericRepository<FuncionarioServico>
    {
        IQueryable<FuncionarioServico> GetAllWithDetails();
        Task<FuncionarioServico?> GetByIdWithDetailsAsync(int id);
        Task<bool> ExistFuncionarioServicoAsync(int funcionarioId,int servicoId,int? ignorarId = null);
    }
}
