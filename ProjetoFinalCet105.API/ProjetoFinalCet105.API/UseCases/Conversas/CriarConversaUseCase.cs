using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Conversas
{
    public class CriarConversaUseCase
    {
        private readonly IConversaRepository _conversaRepository;
        private readonly UserManager<User> _userManager;

        public CriarConversaUseCase(
            IConversaRepository conversaRepository,
            UserManager<User> userManager)
        {
            _conversaRepository = conversaRepository;
            _userManager = userManager;
        }

        public async Task<UseCaseResult<ConversaDTO>> ExecuteAsync(string userId, NovaConversaDTO dto)
        {
            // 1. Obter utilizador autenticado
            var utilizador = await _userManager.FindByIdAsync(userId);

            if (utilizador == null)
            {
                return UseCaseResult<ConversaDTO>.Falha(
                    "Utilizador não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            // 2. Verificar role do utilizador autenticado
            var isCliente =
                await _userManager.IsInRoleAsync(
                    utilizador,
                    "Cliente");

            var isFuncionario =
                await _userManager.IsInRoleAsync(
                    utilizador,
                    "Funcionario");

            if (!isCliente && !isFuncionario)
            {
                return UseCaseResult<ConversaDTO>.Falha(
                    "Não tem permissão para iniciar conversas.",
                    TipoErro.Proibido);
            }

            // 3. Obter destinatário
            var destinatario = await _userManager.FindByIdAsync(dto.DestinatarioId);

            if (destinatario == null)
            {
                return UseCaseResult<ConversaDTO>.Falha(
                    "Destinatário não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            // 4. Não permitir conversa consigo próprio
            if (utilizador.Id == destinatario.Id)
            {
                return UseCaseResult<ConversaDTO>.Falha(
                    "Não é possível iniciar uma conversa consigo próprio.");
            }

            string clienteId;
            string funcionarioUserId;

            User cliente;
            User funcionario;

            // 5. Determinar quem é Cliente e quem é Funcionário
            if (isCliente)
            {
                var destinatarioEhFuncionario =
                    await _userManager.IsInRoleAsync(
                        destinatario,
                        "Funcionario");

                if (!destinatarioEhFuncionario)
                {
                    return UseCaseResult<ConversaDTO>.Falha(
                        "O destinatário deve ser um funcionário.");
                }

                clienteId = utilizador.Id;
                funcionarioUserId = destinatario.Id;

                cliente = utilizador;
                funcionario = destinatario;
            }
            else
            {
                var destinatarioEhCliente =
                    await _userManager.IsInRoleAsync(
                        destinatario,
                        "Cliente");

                if (!destinatarioEhCliente)
                {
                    return UseCaseResult<ConversaDTO>.Falha(
                        "O destinatário deve ser um cliente.");
                }

                clienteId = destinatario.Id;
                funcionarioUserId = utilizador.Id;

                cliente = destinatario;
                funcionario = utilizador;
            }

            // 6. Verificar se já existe conversa entre ambos
            var conversaExistente = await _conversaRepository.GetConversaEntreUtilizadoresAsync(clienteId, funcionarioUserId);

            if (conversaExistente != null)
            {
                var respostaExistente = new ConversaDTO
                {
                    Id = conversaExistente.Id,

                    ClienteId = clienteId,
                    ClienteNome = cliente.NomeCompleto,

                    FuncionarioUserId = funcionarioUserId,
                    FuncionarioNome = funcionario.NomeCompleto,

                    DataCriacao = conversaExistente.DataCriacao
                };

                return UseCaseResult<ConversaDTO>.Ok(respostaExistente);
            }

            // 7. Criar conversa
            try
            {
                var conversa = new Conversa
                {
                    ClienteId = clienteId,
                    FuncionarioUserId = funcionarioUserId,
                    DataCriacao = DateTime.Now
                };

                await _conversaRepository.CreateAsync(conversa);

                var resposta = new ConversaDTO
                {
                    Id = conversa.Id,

                    ClienteId = clienteId,
                    ClienteNome = cliente.NomeCompleto,

                    FuncionarioUserId = funcionarioUserId,
                    FuncionarioNome = funcionario.NomeCompleto,

                    DataCriacao = conversa.DataCriacao
                };

                return UseCaseResult<ConversaDTO>.Ok(resposta);
            }
            catch (Exception)
            {
                return UseCaseResult<ConversaDTO>.Falha("Ocorreu um erro ao criar a conversa.");
            }
        }
    }
}
