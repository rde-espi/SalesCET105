
    using global::ProjetoFinalCet105.API.Entities;
    using global::ProjetoFinalCet105.API.Repositories;
    using global::ProjetoFinalCet105.API.UseCases.Common;
    using Microsoft.AspNetCore.Identity;
    

    namespace ProjetoFinalCet105.API.UseCases.Funcionarios
    {
        public class DeleteFuncionarioUseCase
        {
            private readonly IFuncionarioRepository _funcionarioRepository;
            private readonly UserManager<User> _userManager;

            public DeleteFuncionarioUseCase(
                IFuncionarioRepository funcionarioRepository,
                UserManager<User> userManager)
            {
                _funcionarioRepository = funcionarioRepository;
                _userManager = userManager;
            }

            public async Task<UseCaseResult<bool>> ExecuteAsync(int id)
            {
                // 1. Verificar se o funcionário existe
                var funcionario =
                    await _funcionarioRepository.GetByIdAsync(id);

                if (funcionario == null)
                {
                    return UseCaseResult<bool>.Falha(
                        "Funcionário não encontrado.",
                        TipoErro.NaoEncontrado);
                }

                // 2. Obter o utilizador associado
                var user =
                    await _userManager.FindByIdAsync(funcionario.UserId);

                if (user == null)
                {
                    return UseCaseResult<bool>.Falha(
                        "Utilizador associado ao funcionário não encontrado.",
                        TipoErro.NaoEncontrado);
                }

                // 3. Evitar repetir a desativação
                if (!funcionario.Ativo && !user.Ativo)
                {
                    return UseCaseResult<bool>.Falha(
                        "O funcionário já se encontra desativado.");
                }

                try
                {
                    // 4. Desativar funcionário
                    funcionario.Ativo = false;
                    funcionario.Disponivel = false;

                    // 5. Desativar utilizador
                    user.Ativo = false;
                    user.DataAtualizacao = DateTime.Now;

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
                        "Ocorreu um erro ao desativar o funcionário.");
                }
            }
        }
    }

