using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class FuncionarioRepository:GenericRepository<Funcionario>,IFuncionarioRepositoy
    {
        public FuncionarioRepository(DataContext context) :base(context)
        {
            
        }
    }
}
