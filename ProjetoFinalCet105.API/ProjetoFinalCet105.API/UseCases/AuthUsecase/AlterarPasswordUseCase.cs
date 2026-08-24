using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.AuthUsecase
{
    public class AlterarPasswordUseCase
    {
        private readonly UserManager<User> _userManager;

        public AlterarPasswordUseCase(
            UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(
            string userId,
            AlterarPasswordDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PasswordAtual))
            {
                return UseCaseResult<bool>.Falha(
                    "A password atual é obrigatória.");
            }

            if (string.IsNullOrWhiteSpace(dto.NovaPassword))
            {
                return UseCaseResult<bool>.Falha(
                    "A nova password é obrigatória.");
            }

            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Utilizador não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            if (!user.Ativo)
            {
                return UseCaseResult<bool>.Falha(
                    "O utilizador encontra-se desativado.",
                    TipoErro.Proibido);
            }

            var resultado =
                await _userManager.ChangePasswordAsync(
                    user,
                    dto.PasswordAtual,
                    dto.NovaPassword);

            if (!resultado.Succeeded)
            {
                var erros = string.Join(
                    "; ",
                    resultado.Errors.Select(e => e.Description));

                return UseCaseResult<bool>.Falha(erros);
            }

            user.DataAtualizacao = DateTime.Now;

            await _userManager.UpdateAsync(user);

            return UseCaseResult<bool>.Ok(true);
        }
    }
}
