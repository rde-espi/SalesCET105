using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly UserManager<User> _userManager;

        public ClienteRepository(
            UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IList<User>> GetAllClientesAsync()
        {
            return await _userManager
                .GetUsersInRoleAsync("Cliente");
        }
    }
}
