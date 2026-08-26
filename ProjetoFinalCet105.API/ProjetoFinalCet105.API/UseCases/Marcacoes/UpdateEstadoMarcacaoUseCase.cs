using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.NotificacaoService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Marcacoes
{
    public class UpdateEstadoMarcacaoUseCase
    {
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IEstadoMarcacaoRepository _estadoMarcacaoRepository;
        private readonly IHistoricoMarcacaoRepository _historicoMarcacaoRepository;
        private readonly INotificacaoService _notificacaoService;

        public UpdateEstadoMarcacaoUseCase(
            IMarcacaoRepository marcacaoRepository,
            IFuncionarioRepository funcionarioRepository,
            IEstadoMarcacaoRepository estadoMarcacaoRepository,
            IHistoricoMarcacaoRepository historicoMarcacaoRepository,
            INotificacaoService notificacaoService)
        {
            _marcacaoRepository = marcacaoRepository;
            _funcionarioRepository = funcionarioRepository;
            _estadoMarcacaoRepository = estadoMarcacaoRepository;
            _historicoMarcacaoRepository = historicoMarcacaoRepository;
            _notificacaoService = notificacaoService;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(int id,string userId,bool isFuncionario,bool isAdmin,UpdateEstadoMarcacaoDTO dto)
        {
            var marcacao = await _marcacaoRepository.GetByIdAsync(id);

            if (marcacao == null)
            {
                return UseCaseResult<bool>.Falha("Marcação não encontrada.",TipoErro.NaoEncontrado);
            }

            if (isFuncionario && !isAdmin)
            {
                var funcionarioAutenticado = await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return UseCaseResult<bool>.Falha("Funcionário autenticado não encontrado.",TipoErro.Proibido);
                }

                if (marcacao.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return UseCaseResult<bool>.Falha("Não tem permissão para alterar o estado desta marcação.",TipoErro.Proibido);
                }
            }

            var estadoAtual = await _estadoMarcacaoRepository.GetByIdAsync(marcacao.EstadoMarcacaoId);

            if (estadoAtual == null)
            {
                return UseCaseResult<bool>.Falha("O estado atual da marcação não foi encontrado.", TipoErro.NaoEncontrado);
            }

            var novoEstado =await _estadoMarcacaoRepository.GetByIdAsync(dto.EstadoMarcacaoId);

            if (novoEstado == null)
            {
                return UseCaseResult<bool>.Falha("O estado indicado não existe.", TipoErro.NaoEncontrado);
            }

            if (estadoAtual.Nome == "Cancelada" ||
                estadoAtual.Nome == "Concluida" ||
                estadoAtual.Nome == "Não Compareceu")
            {
                return UseCaseResult<bool>.Falha($"A marcação encontra-se no estado '{estadoAtual.Nome}' e já não pode ser alterada.");
            }

            if (novoEstado.Nome == "Cancelada")
            {
                return UseCaseResult<bool>.Falha("Para cancelar uma marcação deve utilizar o endpoint de cancelamento.");
            }

            var transicaoValida =
                (estadoAtual.Nome == "Pendente" &&
                 novoEstado.Nome == "Confirmada")
                ||
                (estadoAtual.Nome == "Confirmada" &&
                 (novoEstado.Nome == "Concluida" ||
                  novoEstado.Nome == "Não Compareceu"));

            if (!transicaoValida)
            {
                return UseCaseResult<bool>.Falha($"Não é possível alterar o estado de '{estadoAtual.Nome}' para '{novoEstado.Nome}'.");
            }

            try
            {
                marcacao.EstadoMarcacaoId = novoEstado.Id;
                marcacao.DataAtualizacao = DateTime.Now;

                await _marcacaoRepository.UpdateAsync(marcacao);

                var historico = new HistoricoMarcacao
                {
                    MarcacaoId = marcacao.Id,
                    UserId = userId,
                    Acao = "Alteração de estado",
                    Descricao =
                        $"Estado alterado de '{estadoAtual.Nome}' para '{novoEstado.Nome}'.",
                    DataAlteracao = DateTime.Now
                };

                await _historicoMarcacaoRepository.CreateAsync(historico);
                try
                {
                    await _notificacaoService.NotificarEstadoMarcacaoAsync(
                        marcacao.ClienteId,
                        novoEstado.Nome,
                        marcacao.DataHoraInicio);
                }
                catch
                {
                    // Falha na notificação não deve anular
                    // a alteração do estado da marcação.
                }

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception)
            {
                return UseCaseResult<bool>.Falha("Ocorreu um erro ao alterar o estado da marcação.");
            }
        }
    }
}
