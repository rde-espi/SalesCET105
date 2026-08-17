using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class FuncionarioServicoRepository:GenericRepository<FuncionarioServico>,IFuncionarioServicoRepository
    {
        public FuncionarioServicoRepository(DataContext context): base(context)
        {
            
        }
    }
}
