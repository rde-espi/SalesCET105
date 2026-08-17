using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class CategoriaRepository:GenericRepository<Categoria>,ICategoriaRepository
    {
        public CategoriaRepository(DataContext context): base(context)
        {
            
        }
    }
}
