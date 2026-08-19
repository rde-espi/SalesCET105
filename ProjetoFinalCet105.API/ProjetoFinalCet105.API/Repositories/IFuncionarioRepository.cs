using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IFuncionarioRepository: IGenericRepository<Funcionario>
    {
        IQueryable<Funcionario> GetAllFuncionariosWithUser();
        Task<Funcionario?>GetFuncionarioByIdAsync(int id);
        Task<Funcionario?> GetFuncionarioByUserIdAsync(string userId);
    }
}
