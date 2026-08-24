using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.HorarioFuncionarioService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.HorariosFuncionarios
{
    public class DeleteHorarioFuncionarioUseCase
    {
        private readonly IHorarioFuncionarioRepository _horarioFuncionarioRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IHorarioFuncionarioService _horarioFuncionarioService;

        public DeleteHorarioFuncionarioUseCase(
            IHorarioFuncionarioRepository horarioFuncionarioRepository,
            IFuncionarioRepository funcionarioRepository,
                IHorarioFuncionarioService horarioFuncionarioService)

        {
            _horarioFuncionarioRepository = horarioFuncionarioRepository;
            _funcionarioRepository = funcionarioRepository;
            _horarioFuncionarioService = horarioFuncionarioService;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(
            int id,
            string userId,
            bool isFuncionario,
            bool isAdmin)
        {
            // Verificar se o horário existe
            var horario =
                await _horarioFuncionarioRepository.GetByIdAsync(id);

            if (horario == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Horário não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            // Verificar autorização
            var autorizacaoResult =
                await ValidarAutorizacaoAsync(
                    horario,
                    userId,
                    isFuncionario,
                    isAdmin);

            if (!autorizacaoResult.Sucesso)
            {
                return autorizacaoResult;
            }

            // Se já estiver inativo, não há nada para desativar
            if (!horario.Ativo)
            {
                return UseCaseResult<bool>.Falha(
                    "O horário já se encontra inativo.");
            }

            // Verificar se existem marcações confirmadas futuras
            // associadas a este horário
            var existeMarcacaoConfirmada =
                await _horarioFuncionarioService
                    .ExistemMarcacoesConfirmadasNoHorarioAsync(
                        horario.FuncionarioId,
                        horario.DiaSemana,
                        horario.HoraInicio,
                        horario.HoraFim);

            if (existeMarcacaoConfirmada)
            {
                return UseCaseResult<bool>.Falha(
                    "Não é possível desativar este horário porque existem marcações confirmadas futuras associadas a este período.",
                    TipoErro.Conflito);
            }

            // Soft delete
            try
            {
                horario.Ativo = false;

                await _horarioFuncionarioRepository
                    .UpdateAsync(horario);

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception)
            {
                return UseCaseResult<bool>.Falha(
                    "Ocorreu um erro ao desativar o horário.");
            }
        }

        private async Task<UseCaseResult<bool>> ValidarAutorizacaoAsync(
            HorarioFuncionario horario,
            string userId,
            bool isFuncionario,
            bool isAdmin)
        {
            // Admin pode desativar qualquer horário
            if (isAdmin)
            {
                return UseCaseResult<bool>.Ok(true);
            }

            // Funcionário apenas o próprio horário
            if (isFuncionario)
            {
                var funcionario =
                    await _funcionarioRepository
                        .GetFuncionarioByUserIdAsync(userId);

                if (funcionario == null)
                {
                    return UseCaseResult<bool>.Falha(
                        "Funcionário autenticado não encontrado.",
                        TipoErro.Proibido);
                }

                if (funcionario.Id != horario.FuncionarioId)
                {
                    return UseCaseResult<bool>.Falha(
                        "Não tem permissão para desativar este horário.",
                        TipoErro.Proibido);
                }

                return UseCaseResult<bool>.Ok(true);
            }

            return UseCaseResult<bool>.Falha(
                "Utilizador sem permissão.",
                TipoErro.Proibido);
        }
    }
}
