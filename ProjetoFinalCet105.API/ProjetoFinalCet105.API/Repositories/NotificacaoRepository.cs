using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class NotificacaoRepository:GenericRepository<Notificacao>,INotificaçãoRepository
    {
        public NotificacaoRepository(DataContext context):base(context)
        {
            
        }
    }
}
