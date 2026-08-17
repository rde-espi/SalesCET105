using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class HorarioFuncionarioRepository:GenericRepository<HorarioFuncionario>, IHorarioFuncionarioRepository
    {
        public HorarioFuncionarioRepository(DataContext context) :base(context)
        {
            
        }
    }
}
