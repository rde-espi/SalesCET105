using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Repositories;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ProjetoFinalCet105.API.UseCases.Conversas.SignalR.Hubs
{
    [Authorize(Roles = "Cliente,Funcionario")]
    public class ChatHub : Hub
    {
        private readonly IConversaRepository _conversaRepository;

        public ChatHub(
            IConversaRepository conversaRepository)
        {
            _conversaRepository = conversaRepository;
        }

        public async Task EntrarNaConversa(int conversaId)
        {
            var userId =
                Context.User?.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                throw new HubException(
                    "Utilizador não autenticado.");
            }

            var conversa =
                await _conversaRepository
                    .GetByIdAsync(conversaId);

            if (conversa == null)
            {
                throw new HubException(
                    "Conversa não encontrada.");
            }

            var pertenceAConversa =
                conversa.ClienteId == userId ||
                conversa.FuncionarioUserId == userId;

            if (!pertenceAConversa)
            {
                throw new HubException(
                    "Não tem permissão para aceder a esta conversa.");
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"conversa-{conversaId}");
        }

        public async Task SairDaConversa(int conversaId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"conversa-{conversaId}");
        }
    }
}
