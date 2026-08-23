using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Cliente
{
    public class UpdateClienteUseCase
    {
        private readonly UserManager<User> _userManager;

        public UpdateClienteUseCase(
            UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(
            string id,
            string userId,
            bool isCliente,
            bool isAdmin,
            UpdateClienteDTO dto)
        {
            // 1. Verificar se o utilizador existe
            var cliente = await _userManager.FindByIdAsync(id);

            if (cliente == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Cliente não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            // 2. Confirmar que o utilizador é Cliente
            if (!await _userManager.IsInRoleAsync(cliente, "Cliente"))
            {
                return UseCaseResult<bool>.Falha(
                    "O utilizador indicado não é um cliente.",
                    TipoErro.NaoEncontrado);
            }

            // 3. Cliente só pode alterar o próprio perfil
            if (isCliente && !isAdmin)
            {
                if (id != userId)
                {
                    return UseCaseResult<bool>.Falha(
                        "Não tem permissão para alterar este cliente.",
                        TipoErro.Proibido);
                }
            }

            // 4. Validar campos obrigatórios
            if (string.IsNullOrWhiteSpace(dto.NomeCompleto))
            {
                return UseCaseResult<bool>.Falha(
                    "O nome completo é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return UseCaseResult<bool>.Falha(
                    "O email é obrigatório.");
            }

            // 5. Se o email mudou, verificar se já existe
            if (!string.Equals(
                cliente.Email,
                dto.Email,
                StringComparison.OrdinalIgnoreCase))
            {
                var userComEmail =
                    await _userManager.FindByEmailAsync(dto.Email);

                if (userComEmail != null &&
                    userComEmail.Id != cliente.Id)
                {
                    return UseCaseResult<bool>.Falha(
                        "Já existe outro utilizador com este email.",
                        TipoErro.Conflito);
                }
            }

            try
            {
                // 6. Atualizar dados pessoais
                cliente.NomeCompleto = dto.NomeCompleto;
                cliente.Email = dto.Email;
                cliente.UserName = dto.Email;
                cliente.PhoneNumber = dto.Telefone;
                cliente.FotografiaUrl = dto.FotografiaUrl;
                cliente.DataAtualizacao = DateTime.Now;

                // 7. Só Admin pode ativar/desativar cliente
                if (isAdmin && dto.Ativo.HasValue)
                {
                    cliente.Ativo = dto.Ativo.Value;
                }

                var resultado =
                    await _userManager.UpdateAsync(cliente);

                if (!resultado.Succeeded)
                {
                    var erros = string.Join(
                        "; ",
                        resultado.Errors.Select(e => e.Description));

                    return UseCaseResult<bool>.Falha(erros);
                }

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception)
            {
                return UseCaseResult<bool>.Falha(
                    "Ocorreu um erro ao alterar o cliente.");
            }
        }
    }
}
