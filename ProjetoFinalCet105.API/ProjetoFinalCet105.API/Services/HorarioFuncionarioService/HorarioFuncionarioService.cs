using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.Services.HorarioFuncionarioService
{
    public class HorarioFuncionarioService : IHorarioFuncionarioService
    {
        private readonly IHorarioFuncionarioRepository _horarioFuncionarioRepository;
        private readonly IMarcacaoRepository _marcacaoRepository;

        public HorarioFuncionarioService(IHorarioFuncionarioRepository horarioFuncionarioRepository, IMarcacaoRepository marcacaoRepository)
        {
            _horarioFuncionarioRepository =
                horarioFuncionarioRepository;
            _marcacaoRepository = marcacaoRepository;
        }

        public UseCaseResult<bool> ValidarPeriodo(
            TimeSpan horaInicio,
            TimeSpan horaFim)
        {
            if (horaFim <= horaInicio)
            {
                return UseCaseResult<bool>.Falha(
                    "A hora de fim deve ser posterior à hora de início.");
            }

            return UseCaseResult<bool>.Ok(true);
        }

        public async Task<bool> ExisteSobreposicaoAsync(
            int funcionarioId,
            DayOfWeek diaSemana,
            TimeSpan horaInicio,
            TimeSpan horaFim,
            int? ignorarId = null)
        {
            return await _horarioFuncionarioRepository
                .ExisteSobreposicaoAsync(
                    funcionarioId,
                    diaSemana,
                    horaInicio,
                    horaFim,
                    ignorarId);
        }
        public async Task<UseCaseResult<bool>> ValidarMarcacoesConfirmadasFuturasAsync(
    int funcionarioId,
    DayOfWeek diaSemana,
    TimeSpan horaInicio,
    TimeSpan horaFim)
        {
            var agora = DateTime.Now;

            var marcacoesConfirmadas = await _marcacaoRepository
                .GetAllWithDetails()
                .Where(m =>
                    m.FuncionarioId == funcionarioId &&
                    m.EstadoMarcacao.Nome == "Confirmada" &&
                    m.DataHoraInicio > agora)
                .ToListAsync();

            // A partir daqui já estamos em memória
            var marcacoesDoDia = marcacoesConfirmadas
                .Where(m => m.DataHoraInicio.DayOfWeek == diaSemana)
                .ToList();

            var existeMarcacaoForaHorario = marcacoesDoDia.Any(m =>
                m.DataHoraInicio.TimeOfDay < horaInicio ||
                m.DataHoraFim.TimeOfDay > horaFim);

            if (existeMarcacaoForaHorario)
            {
                return UseCaseResult<bool>.Falha(
                    "Não é possível alterar o horário porque existem marcações confirmadas futuras que ficariam fora do novo período.",
                    TipoErro.Conflito);
            }

            return UseCaseResult<bool>.Ok(true);
        }

        public async Task<bool> ExistemMarcacoesConfirmadasNoHorarioAsync(int funcionarioId,DayOfWeek diaSemana,TimeSpan horaInicio,TimeSpan horaFim)
        {
            var agora = DateTime.Now;

            return await _marcacaoRepository
                .GetAllWithDetails()
                .AnyAsync(m =>
                    m.FuncionarioId == funcionarioId &&
                    m.EstadoMarcacao.Nome == "Confirmada" &&
                    m.DataHoraInicio > agora &&
                    m.DataHoraInicio.DayOfWeek == diaSemana &&
                    m.DataHoraInicio.TimeOfDay >= horaInicio &&
                    m.DataHoraFim.TimeOfDay <= horaFim);
        }
    }
}
