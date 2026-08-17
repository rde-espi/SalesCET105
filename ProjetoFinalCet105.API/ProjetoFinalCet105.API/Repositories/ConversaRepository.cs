using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class ConversaRepository:GenericRepository<Conversa>,IConversaRepository
    {
        public ConversaRepository(DataContext context) : base(context)
        {
            
        }
    }
}
