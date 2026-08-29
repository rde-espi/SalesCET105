using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.GoogleCalendarService;
using ProjetoFinalCet105.API.Services.MarcacaoService;
using ProjetoFinalCet105.API.Services.NotificacaoService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Marcacoes
{
    public class UpdateMarcacaoUseCase
    {
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IServicoRepository _servicoRepository;
        private readonly IEstadoMarcacaoRepository _estadoMarcacaoRepository;
        private readonly IHistoricoMarcacaoRepository _historicoMarcacaoRepository;
        private readonly IMarcacaoService _marcacaoService;
        private readonly INotificacaoService _notificacaoService;
        private readonly IGoogleCalendarSyncService _googleCalendarSyncService;
        private readonly ILogger<UpdateMarcacaoUseCase> _logger;

        public UpdateMarcacaoUseCase(
            IMarcacaoRepository marcacaoRepository,
            IFuncionarioRepository funcionarioRepository,
            IServicoRepository servicoRepository,
            IEstadoMarcacaoRepository estadoMarcacaoRepository,
            IHistoricoMarcacaoRepository historicoMarcacaoRepository,
            IMarcacaoService marcacaoService,
            INotificacaoService notificacaoService,
            IGoogleCalendarSyncService googleCalendarSyncService,
            ILogger<UpdateMarcacaoUseCase> logger)
        {
            _marcacaoRepository = marcacaoRepository;
            _funcionarioRepository = funcionarioRepository;
            _servicoRepository = servicoRepository;
            _estadoMarcacaoRepository = estadoMarcacaoRepository;
            _historicoMarcacaoRepository = historicoMarcacaoRepository;
            _marcacaoService = marcacaoService;
            _notificacaoService = notificacaoService;
            _googleCalendarSyncService = googleCalendarSyncService;
            _logger = logger;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(int id, string userId, bool isCliente, bool isFuncionario, bool isAdmin, UpdateMarcacaoDTO dto)
        {
            string quemAlterou = "";

            var marcacaoAtual = await _marcacaoRepository.GetByIdAsync(id);

            if (marcacaoAtual == null)
            {
                return UseCaseResult<bool>.Falha("Marcação não encontrada.", TipoErro.NaoEncontrado);
            }

            int funcionarioId = marcacaoAtual!.FuncionarioId;

            if (isCliente && !isAdmin)
            {
                if (marcacaoAtual.ClienteId != userId)
                {
                    return UseCaseResult<bool>.Falha("Não tem permissão para alterar esta marcação.", TipoErro.Proibido);
                }
                quemAlterou = marcacaoAtual.Cliente.NomeCompleto;
            }

            if (isFuncionario && !isAdmin)
            {
                var funcionarioAutenticado = await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return UseCaseResult<bool>.Falha("Funcionário autenticado não encontrado.",TipoErro.Proibido);
                }

                if (marcacaoAtual.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return UseCaseResult<bool>.Falha("Não tem permissão para alterar esta marcação.",TipoErro.Proibido);
                }

                quemAlterou = funcionarioAutenticado.User.NomeCompleto;
            }

            if (isAdmin)
            {
                quemAlterou = "Administrador";
            }
            
            var servicoAnteriorId = marcacaoAtual.ServicoId;
            var dataInicioAnterior = marcacaoAtual.DataHoraInicio;
            var observacoesAnterior = marcacaoAtual.Observacoes;

            var estadoMarcacao = await _estadoMarcacaoRepository.GetByIdAsync(marcacaoAtual.EstadoMarcacaoId);

            if (estadoMarcacao == null)
            {
                return UseCaseResult<bool>.Falha("Estado da marcação não encontrado.", TipoErro.NaoEncontrado);
            }

            if (estadoMarcacao.Nome == "Cancelada" ||
                estadoMarcacao.Nome == "Concluida" ||
                estadoMarcacao.Nome == "Não Compareceu")
            {
                return UseCaseResult<bool>.Falha(
                    $"Não é possível alterar uma marcação com o estado '{estadoMarcacao.Nome}'.");
            }

            var funcionario = await _funcionarioRepository.GetByIdAsync(funcionarioId);

            if (funcionario == null)
            {
                return UseCaseResult<bool>.Falha("O funcionário indicado não existe.", TipoErro.NaoEncontrado);
            }

            if (!funcionario.Ativo || !funcionario.Disponivel)
            {
                return UseCaseResult<bool>.Falha("O funcionário indicado não está disponível.");
            }

            var servico = await _servicoRepository.GetByIdAsync(dto.ServicoId);

            if (servico == null)
            {
                return UseCaseResult<bool>.Falha("O serviço indicado não existe.", TipoErro.NaoEncontrado);
            }

            if (!servico.Disponivel)
            {
                return UseCaseResult<bool>.Falha("O serviço indicado não está disponível.");
            }

            var funcionarioServico = await _marcacaoService.GetFuncionarioServicoAsync(funcionarioId, dto.ServicoId);

            if (funcionarioServico == null)
            {
                return UseCaseResult<bool>.Falha("Não é possível alterar para este serviço, pois o funcionário da marcação não o realiza.");
            }

            if (dto.DataHoraInicio <= DateTime.Now)
            {
                return UseCaseResult<bool>.Falha("Não é possível reagendar para uma data/hora passada.");
            }

            var duracaoMinutos =
                funcionarioServico.DuracaoPersonalizadaMinutos.HasValue &&
                funcionarioServico.DuracaoPersonalizadaMinutos.Value > 0
                ? funcionarioServico.DuracaoPersonalizadaMinutos.Value
                : servico.DuracaoMinutos;

            if (duracaoMinutos <= 0)
            {
                return UseCaseResult<bool>.Falha("A duração do serviço deve ser superior a zero.");
            }

            var dataHoraFim =
                dto.DataHoraInicio.AddMinutes(duracaoMinutos);

            if (dataHoraFim.Date != dto.DataHoraInicio.Date)
            {
                return UseCaseResult<bool>.Falha("A duração do serviço ultrapassa o horário do mesmo dia.");
            }

            var precoBase =
                funcionarioServico.PrecoPersonalizado.HasValue &&
                funcionarioServico.PrecoPersonalizado.Value > 0
                ? funcionarioServico.PrecoPersonalizado.Value
                : servico.Preco;

            decimal precoFinal = precoBase;

            if (marcacaoAtual.PercentagemDescontoAplicada.HasValue)
            {
                var valorDesconto =
                    Math.Round(
                        precoBase *
                        (marcacaoAtual.PercentagemDescontoAplicada.Value / 100m),
                        2);

                marcacaoAtual.ValorDesconto = valorDesconto;
                precoFinal = precoBase - valorDesconto;
            }
            else
            {
                marcacaoAtual.ValorDesconto = null;
            }

            var horarioValido = await _marcacaoService.HorarioValidoAsync(funcionarioId, dto.DataHoraInicio, dataHoraFim);

            if (!horarioValido)
            {
                return UseCaseResult<bool>.Falha("A marcação está fora do horário de trabalho do funcionário.");
            }

            var indisponivel = await _marcacaoService.ExisteIndisponibilidadeAsync(funcionarioId, dto.DataHoraInicio, dataHoraFim);

            if (indisponivel)
            {
                return UseCaseResult<bool>.Falha("O funcionário está indisponível neste período.");
            }

            if (await _marcacaoService.ExisteSobreposicaoAsync(funcionarioId, dto.DataHoraInicio, dataHoraFim, id))
            {
                return UseCaseResult<bool>.Falha("Já existe uma marcação para este funcionário neste período.", TipoErro.Conflito);
            }

            try
            {
                marcacaoAtual.ServicoId = dto.ServicoId;

                marcacaoAtual.DataHoraInicio = dto.DataHoraInicio;
                marcacaoAtual.DataHoraFim = dataHoraFim;

                marcacaoAtual.Preco = precoFinal;
                marcacaoAtual.Observacoes = dto.Observacoes;

                marcacaoAtual.DataAtualizacao = DateTime.Now;

                await _marcacaoRepository.UpdateAsync(marcacaoAtual);

                var historico = new HistoricoMarcacao
                {
                    MarcacaoId = marcacaoAtual.Id,
                    UserId = userId,
                    Acao = "Alteração da marcação",
                    Descricao =
                    $"Ação feita por: {quemAlterou}; " +
                    $"Serviço: {servicoAnteriorId} -> {marcacaoAtual.ServicoId}; " +
                    $"Data/Hora: {dataInicioAnterior} -> {marcacaoAtual.DataHoraInicio}." +
                    $"Observações: {observacoesAnterior} -> {marcacaoAtual.Observacoes}.",
                    DataAlteracao = DateTime.Now
                };

                await _historicoMarcacaoRepository.CreateAsync(historico);

                try
                {
                    var marcacaoCompleta = await _marcacaoRepository.GetByIdWithDetailsAsync(marcacaoAtual.Id);

                    if (marcacaoCompleta != null)
                    {
                        await _googleCalendarSyncService.SincronizarAtualizacaoMarcacaoAsync( marcacaoCompleta);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning( ex,"A marcação {MarcacaoId} foi alterada, mas ocorreu uma falha ao sincronizar a alteração com o Google Calendar.",marcacaoAtual.Id);
                }

                try
                {
                    await _notificacaoService.NotificarAlteracaoMarcacaoAsync(
                        marcacaoAtual.ClienteId,
                        funcionario.UserId,
                        servico.Nome,
                        marcacaoAtual.DataHoraInicio,
                        isCliente,
                        isFuncionario,
                        isAdmin);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,"A marcação {MarcacaoId} foi alterada, mas ocorreu uma falha ao enviar a notificação.",marcacaoAtual.Id); 
                }

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception)
            {
                return UseCaseResult<bool>.Falha("Ocorreu um erro ao alterar a marcação.");
            }
        }
    }
}
