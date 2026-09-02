using System.Data;

namespace ProjetoFinalCet105.API.UseCases.Marcacoes
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation,IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
}
