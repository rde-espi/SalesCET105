using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class MarcacaoRepository:GenericRepository<Marcacao>, IMarcacaoRepository
    {
        public MarcacaoRepository(DataContext context):base(context)
        {
            
        }
    }
}
