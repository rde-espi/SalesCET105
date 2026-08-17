using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class EstadoMarcacaoRepository:GenericRepository<EstadoMarcacao>,IEstadoMarcacaoRepository
    {
        public EstadoMarcacaoRepository(DataContext context):base(context)
        {
            
        }
    }
}
