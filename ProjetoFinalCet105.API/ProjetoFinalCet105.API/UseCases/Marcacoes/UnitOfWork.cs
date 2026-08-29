using Microsoft.EntityFrameworkCore.Storage;
using ProjetoFinalCet105.API.Data;

namespace ProjetoFinalCet105.API.UseCases.Marcacoes
{
    public class UnitOfWork:IUnitOfWork
    {
        private readonly DataContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(DataContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction =
                await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}
