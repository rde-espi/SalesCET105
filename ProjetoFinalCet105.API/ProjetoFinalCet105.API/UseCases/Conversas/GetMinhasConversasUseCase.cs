using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Conversas
{
    public class GetMinhasConversasUseCase
    {
        private readonly IConversaRepository _conversaRepository;

        public GetMinhasConversasUseCase(
            IConversaRepository conversaRepository)
        {
            _conversaRepository = conversaRepository;
        }

        public async Task<UseCaseResult<List<ConversaDTO>>> ExecuteAsync(string userId, bool isAdmin)
        {
            var query = _conversaRepository.GetAllWithDetails();

            if (!isAdmin)
            {
                query = query.Where(c =>
                    c.ClienteId == userId ||
                    c.FuncionarioUserId == userId);
            }

            var conversas = await query
                .OrderByDescending(c =>
                    c.Mensagens.Any()
                        ? c.Mensagens.Max(m => m.DataEnvio)
                        : c.DataCriacao)
                .Select(c => new ConversaDTO
                {
                    Id = c.Id,

                    ClienteId = c.ClienteId,
                    ClienteNome = c.Cliente.NomeCompleto,

                    FuncionarioUserId = c.FuncionarioUserId,
                    FuncionarioNome = c.Funcionario.NomeCompleto,

                    DataCriacao = c.DataCriacao,

                    Mensagens = new List<MensagemDTO>()
                })
                .ToListAsync();

            return UseCaseResult<List<ConversaDTO>>.Ok(conversas);
        }
    }
}
