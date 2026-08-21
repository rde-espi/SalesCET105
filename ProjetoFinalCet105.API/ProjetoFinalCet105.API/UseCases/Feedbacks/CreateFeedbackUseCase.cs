using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Feedbacks
{
    public class CreateFeedbackUseCase
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IEstadoMarcacaoRepository _estadoMarcacaoRepository;

        public CreateFeedbackUseCase(
            IFeedbackRepository feedbackRepository,
            IMarcacaoRepository marcacaoRepository,
            IEstadoMarcacaoRepository estadoMarcacaoRepository)
        {
            _feedbackRepository = feedbackRepository;
            _marcacaoRepository = marcacaoRepository;
            _estadoMarcacaoRepository = estadoMarcacaoRepository;
        }

        public async Task<UseCaseResult<FeedbackDTO>> ExecuteAsync(
            string userId,
            NovoFeedbackDTO dto)
        {
            // Verificar classificação
            if (dto.Classificacao < 1 || dto.Classificacao > 5)
            {
                return UseCaseResult<FeedbackDTO>.Falha(
                    "A classificação deve estar entre 1 e 5.");
            }

            // Procurar a marcação
            var marcacao = await _marcacaoRepository.GetByIdAsync(dto.MarcacaoId);

            if (marcacao == null)
            {
                return UseCaseResult<FeedbackDTO>.Falha(
                    "Marcação não encontrada.",
                    TipoErro.NaoEncontrado);
            }

            // Só o cliente da marcação pode avaliar
            if (marcacao.ClienteId != userId)
            {
                return UseCaseResult<FeedbackDTO>.Falha(
                    "Não tem permissão para avaliar esta marcação.",
                    TipoErro.Proibido);
            }

            // Verificar estado da marcação
            var estado = await _estadoMarcacaoRepository
                .GetByIdAsync(marcacao.EstadoMarcacaoId);

            if (estado == null)
            {
                return UseCaseResult<FeedbackDTO>.Falha(
                    "Estado da marcação não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            if (estado.Nome != "Concluida")
            {
                return UseCaseResult<FeedbackDTO>.Falha(
                    "Só é possível avaliar uma marcação concluída.");
            }

            // Impedir feedback duplicado
            var jaExisteFeedback =
                await _feedbackRepository
                    .ExisteFeedbackMarcacaoAsync(marcacao.Id);

            if (jaExisteFeedback)
            {
                return UseCaseResult<FeedbackDTO>.Falha(
                    "Já existe feedback para esta marcação.",
                    TipoErro.Conflito);
            }

            try
            {
                var feedback = new Feedback
                {
                    MarcacaoId = marcacao.Id,
                    ClienteId = userId,
                    FuncionarioId = marcacao.FuncionarioId,

                    Classificacao = dto.Classificacao,
                    Comentario = dto.Comentario,

                    DataCriacao = DateTime.Now
                };

                await _feedbackRepository.CreateAsync(feedback);

                var feedbackCompleto =
                    await _feedbackRepository.GetByIdWithDetailsAsync(feedback.Id);

                if (feedbackCompleto == null)
                {
                    return UseCaseResult<FeedbackDTO>.Falha(
                        "Não foi possível obter o feedback criado.",
                        TipoErro.NaoEncontrado);
                }

                var resposta = new FeedbackDTO
                {
                    Id = feedbackCompleto.Id,
                    MarcacaoId = feedbackCompleto.MarcacaoId,

                    ClienteId = feedbackCompleto.ClienteId,
                    ClienteNome = feedbackCompleto.Cliente.NomeCompleto,

                    FuncionarioId = feedbackCompleto.FuncionarioId,
                    FuncionarioNome =
                        feedbackCompleto.Funcionario.User.NomeCompleto,

                    Classificacao = feedbackCompleto.Classificacao,
                    Comentario = feedbackCompleto.Comentario,

                    DataCriacao = feedbackCompleto.DataCriacao
                };

                return UseCaseResult<FeedbackDTO>.Ok(resposta);
            }
            catch (Exception)
            {
                return UseCaseResult<FeedbackDTO>.Falha(
                    "Ocorreu um erro ao criar o feedback.");
            }
        }
    }
}
