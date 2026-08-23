using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Indisponibilidades
{
    public class DeleteIndisponibilidadeUseCase
    {
        private readonly IIndisponibilidadeRepository _indisponibilidadeRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;

        public DeleteIndisponibilidadeUseCase(
            IIndisponibilidadeRepository indisponibilidadeRepository,
            IFuncionarioRepository funcionarioRepository)
        {
            _indisponibilidadeRepository = indisponibilidadeRepository;
            _funcionarioRepository = funcionarioRepository;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(
            int id,
            string userId,
            bool isFuncionario,
            bool isAdmin)
        {
            var indisponibilidade =
                await _indisponibilidadeRepository.GetByIdAsync(id);

            if (indisponibilidade == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Indisponibilidade não encontrada.",
                    TipoErro.NaoEncontrado);
            }

            if (isFuncionario && !isAdmin)
            {
                var funcionarioAutenticado =
                    await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return UseCaseResult<bool>.Falha(
                        "Funcionário autenticado não encontrado.",
                        TipoErro.Proibido);
                }

                if (indisponibilidade.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return UseCaseResult<bool>.Falha(
                        "Não tem permissão para eliminar esta indisponibilidade.",
                        TipoErro.Proibido);
                }

                if (indisponibilidade.DataHoraInicio <= DateTime.Now)
                {
                    return UseCaseResult<bool>.Falha(
                        "Não é possível eliminar uma indisponibilidade que já começou ou terminou.");
                }
            }
            else if (!isAdmin)
            {
                return UseCaseResult<bool>.Falha(
                    "Utilizador sem permissão.",
                    TipoErro.Proibido);
            }

            try
            {
                await _indisponibilidadeRepository.DeleteAsync(indisponibilidade);

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception)
            {
                return UseCaseResult<bool>.Falha(
                    "Ocorreu um erro ao eliminar a indisponibilidade.");
            }
        }
    }
}
