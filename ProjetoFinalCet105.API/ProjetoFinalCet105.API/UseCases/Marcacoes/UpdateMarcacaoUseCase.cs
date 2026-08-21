using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.MarcacaoService;
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

        public UpdateMarcacaoUseCase(
            IMarcacaoRepository marcacaoRepository,
            IFuncionarioRepository funcionarioRepository,
            IServicoRepository servicoRepository,
            IEstadoMarcacaoRepository estadoMarcacaoRepository,
            IHistoricoMarcacaoRepository historicoMarcacaoRepository,
            IMarcacaoService marcacaoService)
        {
            _marcacaoRepository = marcacaoRepository;
            _funcionarioRepository = funcionarioRepository;
            _servicoRepository = servicoRepository;
            _estadoMarcacaoRepository = estadoMarcacaoRepository;
            _historicoMarcacaoRepository = historicoMarcacaoRepository;
            _marcacaoService = marcacaoService;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(int id,string userId,bool isCliente,bool isFuncionario,bool isAdmin,UpdateMarcacaoDTO dto)
        {
            var marcacaoAtual = await _marcacaoRepository.GetByIdAsync(id);

            if (marcacaoAtual == null)
            {
                return UseCaseResult<bool>
                    .Falha("Marcação não encontrada.",TipoErro.NaoEncontrado);
            }

            if (isCliente && !isAdmin)
            {
                if (marcacaoAtual.ClienteId != userId)
                {
                    return UseCaseResult<bool>
                        .Falha("Não tem permissão para alterar esta marcação.",TipoErro.Proibido);
                }
            }

            if (isFuncionario && !isAdmin)
            {
                var funcionarioAutenticado =
                    await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return UseCaseResult<bool>
                        .Falha("Funcionário autenticado não encontrado.", TipoErro.Proibido);
                }

                if (marcacaoAtual.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return UseCaseResult<bool>
                        .Falha("Não tem permissão para alterar esta marcação.", TipoErro.Proibido);
                }

                if (dto.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return UseCaseResult<bool>
                        .Falha("O funcionário não pode transferir a marcação para outra agenda.", TipoErro.Proibido);
                }
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
                await _funcionarioRepository.GetByIdAsync(dto.FuncionarioId);

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

            var funcionarioServico = await _marcacaoService.GetFuncionarioServicoAsync(dto.FuncionarioId,dto.ServicoId);

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
                        
            var horarioValido = await _marcacaoService.HorarioValidoAsync(dto.FuncionarioId, dto.DataHoraInicio, dataHoraFim);

            if (!horarioValido)
            {
                return UseCaseResult<bool>
                    .Falha("A marcação está fora do horário de trabalho do funcionário.");
            }

            var indisponivel = await _marcacaoService.ExisteIndisponibilidadeAsync(dto.FuncionarioId,dto.DataHoraInicio,dataHoraFim);

            if (indisponivel)
            {
                return UseCaseResult<bool>
                    .Falha("O funcionário está indisponível neste período.");
            }

            if (await _marcacaoService.ExisteSobreposicaoAsync(dto.FuncionarioId,dto.DataHoraInicio,dataHoraFim,id))
            {
                return UseCaseResult<bool>
                    .Falha("Já existe uma marcação para este funcionário neste período.",TipoErro.Conflito);
            }

            try
            {
                marcacaoAtual.FuncionarioId = dto.FuncionarioId;
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
                        $"Data/Hora: {dataInicioAnterior} -> {marcacaoAtual.DataHoraInicio}."+
                        $"Observações: {observacoesAnterior} -> {marcacaoAtual.Observacoes}.",
                    DataAlteracao = DateTime.Now
                };

                await _historicoMarcacaoRepository.CreateAsync(historico);

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
