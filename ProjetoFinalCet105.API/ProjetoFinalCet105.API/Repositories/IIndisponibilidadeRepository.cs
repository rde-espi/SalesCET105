using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public interface IIndisponibilidadeRepository:IGenericRepository<Indisponibilidade>
    {
        IQueryable<Indisponibilidade> GetAllIndisponibilidadesWithFuncionario();
        Task<Indisponibilidade?> GetIndisponibilidadeWithFuncionarioByIdAsync(int id);
        Task<bool> ExisteSobreposiçãoAsync(int funcionarioId, DateTime inicio, DateTime fim, int? idIgnorar = null);
    }
}
