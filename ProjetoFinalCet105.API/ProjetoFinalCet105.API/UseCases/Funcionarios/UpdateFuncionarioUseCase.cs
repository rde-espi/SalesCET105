using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Funcionarios
{
    public class UpdateFuncionarioUseCase
    {
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly UserManager<User> _userManager;

        public UpdateFuncionarioUseCase(
            IFuncionarioRepository funcionarioRepository,
            UserManager<User> userManager)
        {
            _funcionarioRepository = funcionarioRepository;
            _userManager = userManager;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(int id,string userId,bool isFuncionario,bool isAdmin,UpdateFuncionarioDTO dto)
        {
            var funcionario = await _funcionarioRepository.GetByIdAsync(id);

            if (funcionario == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Funcionário não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            if (isFuncionario && !isAdmin)
            {
                var funcionarioAutenticado =
                    await _funcionarioRepository
                        .GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return UseCaseResult<bool>.Falha(
                        "Funcionário autenticado não encontrado.",
                        TipoErro.Proibido);
                }

                if (funcionarioAutenticado.Id != funcionario.Id)
                {
                    return UseCaseResult<bool>.Falha(
                        "Não tem permissão para alterar este funcionário.",
                        TipoErro.Proibido);
                }
            }

            var user = await _userManager.FindByIdAsync(funcionario.UserId);           

            if (user == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Utilizador associado ao funcionário não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return UseCaseResult<bool>.Falha(
                    "O email é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(dto.NomeCompleto))
            {
                return UseCaseResult<bool>.Falha(
                    "O nome completo é obrigatório.");
            }

            // Se o email foi alterado, verificar se já pertence a outro utilizador
            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var userComEmail =
                    await _userManager.FindByEmailAsync(dto.Email);

                if (userComEmail != null &&
                    userComEmail.Id != user.Id)
                {
                    return UseCaseResult<bool>.Falha(
                        "Já existe outro utilizador com este email.",
                        TipoErro.Conflito);
                }
            }

            try
            {
                user.NomeCompleto = dto.NomeCompleto;
                user.Email = dto.Email;
                user.UserName = dto.Email;
                user.PhoneNumber = dto.Telefone;
                user.FotografiaUrl = dto.FotografiaUrl;
                user.DataAtualizacao = DateTime.Now;

                funcionario.Biografia = dto.Biografia;



                if (dto.Disponivel.HasValue)
                {
                    funcionario.Disponivel = dto.Disponivel.Value;
                }

                // Só Admin altera dados administrativos
                if (isAdmin)
                {
                    if (dto.DataAdmissao.HasValue)
                    {
                        funcionario.DataAdmissao = dto.DataAdmissao.Value;
                    }

                    if (dto.Ativo.HasValue)
                    {
                        funcionario.Ativo = dto.Ativo.Value;
                    }
                }

                var resultadoUser =
                    await _userManager.UpdateAsync(user);

                if (!resultadoUser.Succeeded)
                {
                    var erros = string.Join(
                        "; ",
                        resultadoUser.Errors.Select(e => e.Description));

                    return UseCaseResult<bool>.Falha(erros);
                }

                await _funcionarioRepository.UpdateAsync(funcionario);

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception)
            {
                return UseCaseResult<bool>.Falha(
                    "Ocorreu um erro ao alterar o funcionário.");
            }
        }
    }
}
