using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;
using ProjetoFinalCet105.API.UseCases.Indisponibilidades;

namespace ProjetoFinalCet105.API.Services.IndisponibilidadeService
{
    public class IndisponibilidadeService : IIndisponibilidadeService
    {
        private readonly IHorarioFuncionarioRepository _horarioFuncionarioRepository;
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IIndisponibilidadeRepository _indisponibilidadeRepository;

        public IndisponibilidadeService(
            IHorarioFuncionarioRepository horarioFuncionarioRepository,
            IMarcacaoRepository marcacaoRepository,
            IIndisponibilidadeRepository indisponibilidadeRepository)
        {
            _horarioFuncionarioRepository = horarioFuncionarioRepository;
            _marcacaoRepository = marcacaoRepository;
            _indisponibilidadeRepository = indisponibilidadeRepository;
        }

        public async Task<List<HorarioFuncionario>> ObterHorariosTrabalhoAsync(
            int funcionarioId,
            DateTime data)
        {
            var diaSemana = data.DayOfWeek;

            return await _horarioFuncionarioRepository
                .GetAll()
                .Where(h =>
                    h.FuncionarioId == funcionarioId &&
                    h.DiaSemana == diaSemana &&
                    h.Ativo)
                .OrderBy(h => h.HoraInicio)
                .ToListAsync();
        }

        public async Task<List<Marcacao>> ObterMarcacoesDoDiaAsync(
            int funcionarioId,
            DateTime data)
        {
            var inicioDia = data.Date;
            var fimDia = inicioDia.AddDays(1);

            return await _marcacaoRepository
                .GetAllWithDetails()
                .Where(m =>
                    m.FuncionarioId == funcionarioId &&
                    m.DataHoraInicio < fimDia &&
                    m.DataHoraFim > inicioDia)
                .ToListAsync();
        }

        public UseCaseResult<bool> ValidarTipoIndisponibilidade(
            bool diaCompleto,
            bool restoDoDia)
        {
            if (diaCompleto && restoDoDia)
            {
                return UseCaseResult<bool>.Falha(
                    "A indisponibilidade não pode ser simultaneamente de dia completo e resto do dia.");
            }

            return UseCaseResult<bool>.Ok(true);
        }

        public UseCaseResult<bool> ValidarConflitoComMarcacoesConfirmadas(
            DateTime inicio,
            DateTime fim,
            List<Marcacao> marcacoesConfirmadas)
        {
            var colide = marcacoesConfirmadas.Any(m =>
                inicio < m.DataHoraFim &&
                fim > m.DataHoraInicio);

            if (colide)
            {
                return UseCaseResult<bool>.Falha(
                    "Não é possível criar/alterar a indisponibilidade porque existem marcações confirmadas neste período.",
                    TipoErro.Conflito);
            }

            return UseCaseResult<bool>.Ok(true);
        }

        public async Task<bool> ExisteSobreposicaoAsync(
            int funcionarioId,
            DateTime inicio,
            DateTime fim,
            int? ignorarId = null)
        {
            return await _indisponibilidadeRepository
                .ExisteSobreposiçãoAsync(
                    funcionarioId,
                    inicio,
                    fim,
                    ignorarId);
        }
                
        public UseCaseResult<PeriodoIndisponibilidade> CalcularPeriodo(
            DateTime dataHoraInicio, 
            DateTime dataHoraFim, 
            bool diaCompleto, 
            bool restoDoDia, 
            List<HorarioFuncionario> horariosTrabalho, 
            List<Marcacao> marcacoesConcluidas)
        {
            var inicio = dataHoraInicio;
            var fim = dataHoraFim;

            var inicioHorarioTrabalho =
                dataHoraInicio.Date
                    .Add(horariosTrabalho.First().HoraInicio);

            var fimHorarioTrabalho =
                dataHoraInicio.Date
                    .Add(horariosTrabalho.Last().HoraFim);

            // DIA COMPLETO
            if (diaCompleto)
            {
                // Se já existirem marcações concluídas,
                // já não faz sentido considerar o dia inteiro indisponível.
                if (marcacoesConcluidas.Any())
                {
                    return UseCaseResult<PeriodoIndisponibilidade>.Falha(
                        "Já existem marcações concluídas neste dia. Utilize a opção 'Resto do dia'.",
                        TipoErro.Conflito);
                }

                inicio = inicioHorarioTrabalho;
                fim = fimHorarioTrabalho;
            }

            // RESTO DO DIA
            else if (restoDoDia)
            {
                // Só pode ser usado no dia atual
                if (dataHoraInicio.Date != DateTime.Today)
                {
                    return UseCaseResult<PeriodoIndisponibilidade>.Falha(
                        "A opção 'Resto do dia' só pode ser utilizada para o dia atual.");
                }

                // O horário de trabalho já tem de ter começado
                if (DateTime.Now < inicioHorarioTrabalho)
                {
                    return UseCaseResult<PeriodoIndisponibilidade>.Falha(
                        $"Ainda não é possível marcar o resto do dia como indisponível. " +
                        $"O horário de trabalho começa às {inicioHorarioTrabalho:HH:mm}.");
                }

                // Se o horário já terminou, já não existe resto do dia
                if (DateTime.Now >= fimHorarioTrabalho)
                {
                    return UseCaseResult<PeriodoIndisponibilidade>.Falha(
                        "O horário de trabalho deste dia já terminou.");
                }

                var ultimaConcluida = marcacoesConcluidas
                    .OrderByDescending(m => m.DataHoraFim)
                    .FirstOrDefault();

                // Por defeito, começa agora
                inicio = DateTime.Now;

                
                if (ultimaConcluida != null &&
                    ultimaConcluida.DataHoraFim > inicio)
                {
                    inicio = ultimaConcluida.DataHoraFim;
                }

                // Segurança extra: nunca antes do início do horário
                if (inicio < inicioHorarioTrabalho)
                {
                    inicio = inicioHorarioTrabalho;
                }

                fim = fimHorarioTrabalho;
            }

            // INDISPONIBILIDADE NORMAL
            else
            {
                if (fim <= inicio)
                {
                    return UseCaseResult<PeriodoIndisponibilidade>.Falha(
                        "A data/hora de fim deve ser posterior à data/hora de início.");
                }

                // Tem de estar dentro do horário de trabalho
                if (inicio < inicioHorarioTrabalho ||
                    fim > fimHorarioTrabalho)
                {
                    return UseCaseResult<PeriodoIndisponibilidade>.Falha(
                        $"A indisponibilidade deve estar dentro do horário de trabalho " +
                        $"({inicioHorarioTrabalho:HH:mm} - {fimHorarioTrabalho:HH:mm}).");
                }

                var ultimaConcluida = marcacoesConcluidas
                    .OrderByDescending(m => m.DataHoraFim)
                    .FirstOrDefault();

                // Não pode começar antes do fim da última marcação concluída
                if (ultimaConcluida != null &&
                    inicio < ultimaConcluida.DataHoraFim)
                {
                    return UseCaseResult<PeriodoIndisponibilidade>.Falha(
                        $"A indisponibilidade só pode começar após o fim da última marcação concluída, " +
                        $"às {ultimaConcluida.DataHoraFim:HH:mm}.");
                }
            }

            // VALIDAÇÃO FINAL DO PERÍODO
            if (fim <= inicio)
            {
                return UseCaseResult<PeriodoIndisponibilidade>.Falha(
                    "Não existe período de trabalho disponível para esta indisponibilidade.");
            }

            return UseCaseResult<PeriodoIndisponibilidade>.Ok(
                new PeriodoIndisponibilidade
                {
                    Inicio = inicio,
                    Fim = fim
                });
        }
    }
}
