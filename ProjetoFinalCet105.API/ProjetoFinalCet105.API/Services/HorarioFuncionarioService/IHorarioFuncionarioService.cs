using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.Services.HorarioFuncionarioService
{
    public interface IHorarioFuncionarioService
    {
        UseCaseResult<bool> ValidarPeriodo(
            TimeSpan horaInicio,
            TimeSpan horaFim);

        Task<bool> ExisteSobreposicaoAsync(
            int funcionarioId,
            DayOfWeek diaSemana,
            TimeSpan horaInicio,
            TimeSpan horaFim,
            int? ignorarId = null);

        Task<UseCaseResult<bool>> ValidarMarcacoesConfirmadasFuturasAsync(
            int funcionarioId,
            DayOfWeek diaSemana,
            TimeSpan horaInicio,
            TimeSpan horaFim);

        Task<bool> ExistemMarcacoesConfirmadasNoHorarioAsync(
            int funcionarioId,
            DayOfWeek diaSemana,
            TimeSpan horaInicio,
            TimeSpan horaFim);
    }
}
