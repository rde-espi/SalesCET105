using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class FuncionarioCompetenciaRepository:GenericRepository<FuncionarioCompetencia>,IFuncionarioCompetenciaRepository
    {
        public FuncionarioCompetenciaRepository(DataContext context):base(context)
        {
            
        }
    }
}
