using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.GoogleCalendarService;
using ProjetoFinalCet105.API.Services.NotificacaoService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Marcacoes
{
    public class CancelarMarcacaoUseCase
    {
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IEstadoMarcacaoRepository _estadoMarcacaoRepository;
        private readonly IHistoricoMarcacaoRepository _historicoMarcacaoRepository;
        private readonly INotificacaoService _notificacaoService;
        private readonly IGoogleCalendarSyncService _googleCalendarSyncService;

        public CancelarMarcacaoUseCase(
            IMarcacaoRepository marcacaoRepository,
            IFuncionarioRepository funcionarioRepository,
            IEstadoMarcacaoRepository estadoMarcacaoRepository,
            IHistoricoMarcacaoRepository historicoMarcacaoRepository,
            INotificacaoService notificacaoService,
            IGoogleCalendarSyncService googleCalendarSyncService)
        {
            _marcacaoRepository = marcacaoRepository;
            _funcionarioRepository = funcionarioRepository;
            _estadoMarcacaoRepository = estadoMarcacaoRepository;
            _historicoMarcacaoRepository = historicoMarcacaoRepository;
            _notificacaoService = notificacaoService;
            _googleCalendarSyncService = googleCalendarSyncService;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(int id,string userId,bool isCliente,bool isFuncionario,bool isAdmin)
        {
            var marcacao = await _marcacaoRepository.GetByIdAsync(id);

            if (marcacao == null)
            {
                return UseCaseResult<bool>
                    .Falha("Marcação não encontrada.", TipoErro.NaoEncontrado);
            }

            if (isCliente && !isAdmin)
            {
                if (marcacao.ClienteId != userId)
                {
                    return UseCaseResult<bool>
                        .Falha("Não tem permissão para cancelar esta marcação.", TipoErro.Proibido);
                }
            }

            if (isFuncionario && !isAdmin)
            {
                var funcionarioAutenticado =
                    await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return UseCaseResult<bool>
                        .Falha("Funcionário autenticado não encontrado.");
                }

                if (marcacao.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return UseCaseResult<bool>
                        .Falha("Não tem permissão para cancelar esta marcação.");
                }
            }

            var estadoCancelada = await _estadoMarcacaoRepository
                .GetAll()
                .FirstOrDefaultAsync(e => e.Nome == "Cancelada");

            if (estadoCancelada == null)
            {
                return UseCaseResult<bool>
                    .Falha("O estado Cancelada não foi encontrado.");
            }

            var estadoAtual = await _estadoMarcacaoRepository
                .GetByIdAsync(marcacao.EstadoMarcacaoId);

            if (estadoAtual == null)
            {
                return UseCaseResult<bool>
                    .Falha("O estado atual da marcação não foi encontrado.");
            }

            if (estadoAtual.Nome == "Concluida" ||
                estadoAtual.Nome == "Não Compareceu")
            {
                return UseCaseResult<bool>.Falha(
                    $"Não é possível cancelar uma marcação com o estado '{estadoAtual.Nome}'.");
            }

            if (estadoAtual.Nome == "Cancelada")
            {
                return UseCaseResult<bool>
                    .Falha("A marcação já se encontra cancelada.");
            }
            var funcionario = await _funcionarioRepository.GetFuncionarioByIdAsync(marcacao.FuncionarioId);

            if (funcionario == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Funcionário da marcação não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            try
            {
                marcacao.EstadoMarcacaoId = estadoCancelada.Id;
                marcacao.DataAtualizacao = DateTime.Now;

                await _marcacaoRepository.UpdateAsync(marcacao);

                var historico = new HistoricoMarcacao
                {
                    MarcacaoId = marcacao.Id,
                    UserId = userId,
                    Acao = "Cancelamento",
                    Descricao =
                        $"Marcação cancelada. Estado anterior: '{estadoAtual.Nome}'.",
                    DataAlteracao = DateTime.Now
                };

                await _historicoMarcacaoRepository.CreateAsync(historico);

                try
                {
                    var marcacaoCompleta = await _marcacaoRepository.GetByIdWithDetailsAsync(marcacao.Id);

                    if (marcacaoCompleta != null)
                    {
                        await _googleCalendarSyncService.SincronizarCancelamentoMarcacaoAsync( marcacaoCompleta);
                    }
                }
                catch
                {
                    // Falha no Google Calendar não deve
                    // anular o cancelamento.
                }

                try
                {
                    await _notificacaoService.NotificarCancelamentoMarcacaoAsync(
                        marcacao.ClienteId,
                        funcionario.UserId,
                        marcacao.DataHoraInicio,
                        isCliente,
                        isFuncionario,
                        isAdmin);
                }
                catch
                {
                    // Falha na notificação não deve anular
                    // o cancelamento da marcação.
                }

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception)
            {
                return UseCaseResult<bool>
                    .Falha("Ocorreu um erro ao cancelar a marcação.");
            }
        }
    }
}
