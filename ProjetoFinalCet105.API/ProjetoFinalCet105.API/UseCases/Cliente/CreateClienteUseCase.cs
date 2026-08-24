using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Services.AuthService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Cliente
{
    public class CreateClienteUseCase
    {
        private readonly UserManager<User> _userManager;
        private readonly IAuthService _authService;

        public CreateClienteUseCase(UserManager<User> userManager, IAuthService authService)
        {
            _userManager = userManager;
            _authService = authService;
        }

        public async Task<UseCaseResult<ClienteDTO>> ExecuteAsync(
            NovoClienteDTO dto)
        {
            var userExistente =
                await _userManager.FindByEmailAsync(dto.Email);

            if (userExistente != null)
            {
                return UseCaseResult<ClienteDTO>.Falha(
                    "Já existe um utilizador com este email.",
                    TipoErro.Conflito);
            }

            var user = new User
            {
                NomeCompleto = dto.NomeCompleto,
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.Telefone,
                FotografiaUrl = dto.FotografiaUrl,
                Ativo = true,
                DataCriacao = DateTime.Now
            };

            var resultado =
                await _userManager.CreateAsync(
                    user,
                    dto.Password);

            if (!resultado.Succeeded)
            {
                var erros = string.Join(
                    "; ",
                    resultado.Errors.Select(e => e.Description));

                return UseCaseResult<ClienteDTO>.Falha(erros);
            }

            var resultadoRole =
                await _userManager.AddToRoleAsync(
                    user,
                    "Cliente");

            if (!resultadoRole.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                var erros = string.Join(
                    "; ",
                    resultadoRole.Errors.Select(e => e.Description));

                return UseCaseResult<ClienteDTO>.Falha(erros);
            }

            var resposta = new ClienteDTO
            {
                Id = user.Id,
                NomeCompleto = user.NomeCompleto,
                Email = user.Email,
                Telefone = user.PhoneNumber,
                FotografiaUrl = user.FotografiaUrl,
                Ativo = user.Ativo,
                DataCriacao = user.DataCriacao,
                DataAtualizacao = user.DataAtualizacao
            };
            try
            {
                await _authService.EnviarConfirmacaoEmailAsync(user);
            }
            catch(Exception) 
            {
            }

            return UseCaseResult<ClienteDTO>.Ok(resposta);
        }
    }
}
