using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class MensagemRepository:GenericRepository<Mensagem>, IMensagemRepository
    {
        public MensagemRepository(DataContext context):base(context)
        {
            
        }
    }
}
