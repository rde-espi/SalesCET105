using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Services.AuthService
{
    public interface IAuthService
    {
        Task<LoginResponseDTO> GerarRespostaLoginAsync(User user);

        Task EnviarConfirmacaoEmailAsync(User user);
    }
}
