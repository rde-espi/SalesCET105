using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class ServicoRepository:GenericRepository<Servico>, IServicoRepository
    {
        public ServicoRepository(DataContext context) : base(context)
        {
            
        }
    }
}
