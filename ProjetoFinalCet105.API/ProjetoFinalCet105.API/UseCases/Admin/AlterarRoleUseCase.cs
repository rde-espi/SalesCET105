using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Admin
{
    public class AlterarRoleUserUseCase
    {
        private readonly UserManager<User> _userManager;
        private readonly IFuncionarioRepository _funcionarioRepository;

        private static readonly string[] RolesPermitidas =
        {
            "Admin",
            "Funcionario",
            "Cliente"
        };

        public AlterarRoleUserUseCase( UserManager<User> userManager, IFuncionarioRepository funcionarioRepository)
        {
            _userManager = userManager;
            _funcionarioRepository = funcionarioRepository;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync( string userId, string adminAtualId, AlterarRoleUserDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NovaRole))
            {
                return UseCaseResult<bool>.Falha("A nova role é obrigatória.");
            }

            var novaRole = RolesPermitidas
                .FirstOrDefault(r => r.Equals( dto.NovaRole.Trim(), StringComparison.OrdinalIgnoreCase));

            if (novaRole == null)
            {
                return UseCaseResult<bool>.Falha( "Role inválida. As roles permitidas são Admin, Funcionario e Cliente.");
            }

            // Impede o Admin de alterar a própria role.
            if (userId == adminAtualId)
            {
                return UseCaseResult<bool>.Falha( "Não pode alterar a sua própria role.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return UseCaseResult<bool>.Falha( "Utilizador não encontrado.", TipoErro.NaoEncontrado);
            }

            var rolesAtuais = await _userManager.GetRolesAsync(user);

            // Se já tem exatamente esta role
            if (rolesAtuais.Count == 1 && rolesAtuais.Contains( novaRole, StringComparer.OrdinalIgnoreCase))
            {
                return UseCaseResult<bool>.Falha($"O utilizador já possui a role {novaRole}.");
            }

            // Se estiver a retirar Admin verifica se existe pelo menos outro Admin.
            if (rolesAtuais.Contains("Admin") && novaRole != "Admin")
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");

                if (admins.Count <= 1)
                {
                    return UseCaseResult<bool>.Falha( "Não é possível remover a role Admin do último administrador do sistema.");
                }
            }

            var funcionario = await _funcionarioRepository.GetFuncionarioByUserIdAsync(user.Id);

            
            if (novaRole != "Funcionario" && funcionario != null && funcionario.Ativo)
            {
                funcionario.Ativo = false;
                funcionario.Disponivel = false;
                funcionario.User = null!;
                await _funcionarioRepository.UpdateAsync(funcionario);
            }

            
            if (novaRole == "Funcionario")
            {
                if (funcionario == null)
                {
                    funcionario = new Funcionario
                    {
                        UserId = user.Id,
                        Biografia = dto.Biografia,
                        DataAdmissao = dto.DataAdmissao ?? DateTime.Today,
                        Disponivel = dto.Disponivel,
                        Ativo = true
                    };

                    await _funcionarioRepository.CreateAsync(funcionario);
                }
                else
                {
                    funcionario.Biografia = dto.Biografia ?? funcionario.Biografia;

                    funcionario.DataAdmissao = dto.DataAdmissao ?? funcionario.DataAdmissao ?? DateTime.Today;

                    funcionario.Disponivel = dto.Disponivel;

                    funcionario.Ativo = true;
                    funcionario.User = null!;

                    await _funcionarioRepository.UpdateAsync(funcionario);
                }
            }

            // Remove todas as roles principais atuais.
            if (rolesAtuais.Any())
            {
                var resultadoRemover =await _userManager.RemoveFromRolesAsync(user, rolesAtuais);

                if (!resultadoRemover.Succeeded)
                {
                    var erros = string.Join("; ", resultadoRemover.Errors.Select(e => e.Description));

                    return UseCaseResult<bool>.Falha(erros);
                }
            }

            // Adiciona a nova role.
            var resultadoAdicionar = await _userManager.AddToRoleAsync(user, novaRole);

            if (!resultadoAdicionar.Succeeded)
            {
                var erros = string.Join( "; ", resultadoAdicionar.Errors.Select(e => e.Description));

                return UseCaseResult<bool>.Falha(erros);
            }

            user.DataAtualizacao = DateTime.Now;

            var resultadoUser = await _userManager.UpdateAsync(user);

            if (!resultadoUser.Succeeded)
            {
                var erros = string.Join( "; ", resultadoUser.Errors.Select(e => e.Description));

                return UseCaseResult<bool>.Falha(erros);
            }

            return UseCaseResult<bool>.Ok(true);
        }
    }
}
