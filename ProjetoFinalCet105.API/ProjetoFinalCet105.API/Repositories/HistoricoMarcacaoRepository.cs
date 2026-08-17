using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class HistoricoMarcacaoRepository:GenericRepository<HistoricoMarcacao>, IHistoricoMarcacaoRepository
    {
        public HistoricoMarcacaoRepository(DataContext context):base(context)
        {
            
        }
    }
}
