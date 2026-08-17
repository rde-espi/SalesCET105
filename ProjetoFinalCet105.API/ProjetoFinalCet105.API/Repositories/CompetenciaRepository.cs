using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class CompetenciaRepository:GenericRepository<Competencia>, ICompetenciaRepository
    {
        public CompetenciaRepository(DataContext context) : base(context)
        {
            
        }
    }
}
