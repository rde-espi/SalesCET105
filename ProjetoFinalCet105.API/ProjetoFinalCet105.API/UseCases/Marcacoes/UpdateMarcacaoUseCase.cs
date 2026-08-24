using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
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

        public UpdateMarcacaoUseCase(
            IMarcacaoRepository marcacaoRepository,
            IFuncionarioRepository funcionarioRepository,
            IServicoRepository servicoRepository,
            IEstadoMarcacaoRepository estadoMarcacaoRepository,
            IHistoricoMarcacaoRepository historicoMarcacaoRepository,
            IMarcacaoService marcacaoService,
            INotificacaoService notificacaoService)
        {
            _marcacaoRepository = marcacaoRepository;
            _funcionarioRepository = funcionarioRepository;
            _servicoRepository = servicoRepository;
            _estadoMarcacaoRepository = estadoMarcacaoRepository;
            _historicoMarcacaoRepository = historicoMarcacaoRepository;
            _marcacaoService = marcacaoService;
            _notificacaoService = notificacaoService;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(int id, string userId, bool isCliente, bool isFuncionario, bool isAdmin, UpdateMarcacaoDTO dto)
        {
            int funcionarioId;

            var marcacaoAtual = await _marcacaoRepository.GetByIdAsync(id);

            if (marcacaoAtual == null)
            {
                return UseCaseResult<bool>
                    .Falha("Marcação não encontrada.", TipoErro.NaoEncontrado);
            }

            if (isCliente && !isAdmin)
            {
                if (marcacaoAtual.ClienteId != userId)
                {
                    return UseCaseResult<bool>
                        .Falha("Não tem permissão para alterar esta marcação.", TipoErro.Proibido);
                }
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

                if (marcacaoAtual.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return UseCaseResult<bool>.Falha(
                        "Não tem permissão para alterar esta marcação.",
                        TipoErro.Proibido);
                }

                funcionarioId = funcionarioAutenticado.Id;
            }
            else
            {
                if (!dto.FuncionarioId.HasValue)
                {
                    return UseCaseResult<bool>.Falha(
                        "É necessário indicar o funcionário da marcação.");
                }

                funcionarioId = dto.FuncionarioId.Value;
            }

            var funcionarioAnteriorId = marcacaoAtual.FuncionarioId;
            var servicoAnteriorId = marcacaoAtual.ServicoId;
            var dataInicioAnterior = marcacaoAtual.DataHoraInicio;
            var observacoesAnterior = marcacaoAtual.Observacoes;

            var estadoMarcacao =
                await _estadoMarcacaoRepository.GetByIdAsync(
                    marcacaoAtual.EstadoMarcacaoId);

            if (estadoMarcacao == null)
            {
                return UseCaseResult<bool>
                    .Falha("Estado da marcação não encontrado.", TipoErro.NaoEncontrado);
            }

            if (estadoMarcacao.Nome == "Cancelada" ||
                estadoMarcacao.Nome == "Concluida" ||
                estadoMarcacao.Nome == "Não Compareceu")
            {
                return UseCaseResult<bool>.Falha(
                    $"Não é possível alterar uma marcação com o estado '{estadoMarcacao.Nome}'.");
            }

            var funcionario =
                await _funcionarioRepository.GetByIdAsync(funcionarioId);

            if (funcionario == null)
            {
                return UseCaseResult<bool>
                    .Falha("O funcionário indicado não existe.", TipoErro.NaoEncontrado);
            }

            if (!funcionario.Ativo || !funcionario.Disponivel)
            {
                return UseCaseResult<bool>
                    .Falha("O funcionário indicado não está disponível.");
            }

            var servico =
                await _servicoRepository.GetByIdAsync(dto.ServicoId);

            if (servico == null)
            {
                return UseCaseResult<bool>
                    .Falha("O serviço indicado não existe.", TipoErro.NaoEncontrado);
            }

            if (!servico.Disponivel)
            {
                return UseCaseResult<bool>
                    .Falha("O serviço indicado não está disponível.");
            }

            var funcionarioServico = await _marcacaoService.GetFuncionarioServicoAsync(funcionarioId, dto.ServicoId);

            if (funcionarioServico == null)
            {
                return UseCaseResult<bool>
                    .Falha("O funcionário indicado não realiza este serviço.");
            }

            if (dto.DataHoraInicio <= DateTime.Now)
            {
                return UseCaseResult<bool>
                    .Falha("Não é possível reagendar para uma data/hora passada.");
            }

            var duracaoMinutos =
                funcionarioServico.DuracaoPersonalizadaMinutos
                ?? servico.DuracaoMinutos;

            if (duracaoMinutos <= 0)
            {
                return UseCaseResult<bool>.Falha(
                    "A duração do serviço deve ser superior a zero.");
            }

            var dataHoraFim =
                dto.DataHoraInicio.AddMinutes(duracaoMinutos);

            if (dataHoraFim.Date != dto.DataHoraInicio.Date)
            {
                return UseCaseResult<bool>
                    .Falha("A duração do serviço ultrapassa o horário do mesmo dia.");
            }

            var preco =
                funcionarioServico.PrecoPersonalizado
                ?? servico.Preco;

            var horarioValido = await _marcacaoService.HorarioValidoAsync(funcionarioId, dto.DataHoraInicio, dataHoraFim);

            if (!horarioValido)
            {
                return UseCaseResult<bool>
                    .Falha("A marcação está fora do horário de trabalho do funcionário.");
            }

            var indisponivel = await _marcacaoService.ExisteIndisponibilidadeAsync(funcionarioId, dto.DataHoraInicio, dataHoraFim);

            if (indisponivel)
            {
                return UseCaseResult<bool>
                    .Falha("O funcionário está indisponível neste período.");
            }

            if (await _marcacaoService.ExisteSobreposicaoAsync(funcionarioId, dto.DataHoraInicio, dataHoraFim, id))
            {
                return UseCaseResult<bool>
                    .Falha("Já existe uma marcação para este funcionário neste período.", TipoErro.Conflito);
            }

            try
            {
                marcacaoAtual.FuncionarioId = funcionarioId;
                marcacaoAtual.ServicoId = dto.ServicoId;

                marcacaoAtual.DataHoraInicio = dto.DataHoraInicio;
                marcacaoAtual.DataHoraFim = dataHoraFim;

                marcacaoAtual.Preco = preco;
                marcacaoAtual.Observacoes = dto.Observacoes;

                marcacaoAtual.DataAtualizacao = DateTime.Now;

                await _marcacaoRepository.UpdateAsync(marcacaoAtual);

                var historico = new HistoricoMarcacao
                {
                    MarcacaoId = marcacaoAtual.Id,
                    UserId = userId,
                    Acao = "Alteração da marcação",
                    Descricao =
                        $"Funcionário: {funcionarioAnteriorId} -> {marcacaoAtual.FuncionarioId}; " +
                        $"Serviço: {servicoAnteriorId} -> {marcacaoAtual.ServicoId}; " +
                        $"Data/Hora: {dataInicioAnterior} -> {marcacaoAtual.DataHoraInicio}." +
                        $"Observações: {observacoesAnterior} -> {marcacaoAtual.Observacoes}.",
                    DataAlteracao = DateTime.Now
                };

                await _historicoMarcacaoRepository.CreateAsync(historico);

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
                catch
                {
                    // Uma falha na notificação não deve anular
                    // uma alteração já realizada com sucesso.
                }

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception)
            {
                return UseCaseResult<bool>
                    .Falha("Ocorreu um erro ao alterar a marcação.");
            }
        }
    }
}
