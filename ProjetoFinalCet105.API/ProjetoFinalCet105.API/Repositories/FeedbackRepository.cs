using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class FeedbackRepository:GenericRepository<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(DataContext context):base(context)
        {
            
        }
    }
}
