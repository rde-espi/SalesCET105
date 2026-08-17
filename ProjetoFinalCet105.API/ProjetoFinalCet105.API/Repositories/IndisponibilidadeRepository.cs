using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class IndisponibilidadeRepository:GenericRepository<Indisponibilidade>,IIndisponibilidadeRepository
    {
        public IndisponibilidadeRepository(DataContext context):base(context)
        {
            
        }
    }
}
