using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Conversas
{
    public class GetConversaByIdUseCase
    {
        private readonly IConversaRepository _conversaRepository;

        public GetConversaByIdUseCase(IConversaRepository conversaRepository)
        {
            _conversaRepository = conversaRepository;
        }

        public async Task<UseCaseResult<ConversaDTO>> ExecuteAsync(int id,string userId, bool isAdmin)
        {
            var conversa =
                await _conversaRepository
                    .GetByIdWithDetailsAsync(id);

            if (conversa == null)
            {
                return UseCaseResult<ConversaDTO>.Falha(
                    "Conversa não encontrada.",
                    TipoErro.NaoEncontrado);
            }

            var pertenceAConversa =
                conversa.ClienteId == userId ||
                conversa.FuncionarioUserId == userId;

            if (!pertenceAConversa && !isAdmin)
            {
                return UseCaseResult<ConversaDTO>.Falha(
                    "Não tem permissão para consultar esta conversa.",
                    TipoErro.Proibido);
            }

            var resposta = new ConversaDTO
            {
                Id = conversa.Id,

                ClienteId = conversa.ClienteId,
                ClienteNome = conversa.Cliente.NomeCompleto,

                FuncionarioUserId = conversa.FuncionarioUserId,
                FuncionarioNome = conversa.Funcionario.NomeCompleto,

                DataCriacao = conversa.DataCriacao,

                Mensagens = conversa.Mensagens
                    .OrderBy(m => m.DataEnvio)
                    .Select(m => new MensagemDTO
                    {
                        Id = m.Id,
                        ConversaId = m.ConversaId,

                        RemetenteId = m.RemetenteId,
                        RemetenteNome = m.Remetente.NomeCompleto,

                        Texto = m.Texto,
                        DataEnvio = m.DataEnvio,

                        Lida = m.Lida,
                        DataLeitura = m.DataLeitura
                    })
                    .ToList()
            };

            return UseCaseResult<ConversaDTO>.Ok(resposta);
        }
    }
}
