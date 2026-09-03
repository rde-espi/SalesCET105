using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.NotificacaoService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Admin
{
    public class ConcederPermissaoAdminTemporariaUseCase
    {
        private readonly UserManager<User> _userManager;
        private readonly IPermissaoAdminTemporariaRepository
            _permissaoRepository;
        private readonly INotificacaoService _notificacaoService;

        public ConcederPermissaoAdminTemporariaUseCase(UserManager<User> userManager, IPermissaoAdminTemporariaRepository permissaoRepository,INotificacaoService notificacaoService)
        {
            _userManager = userManager;
            _permissaoRepository = permissaoRepository;
            _notificacaoService = notificacaoService;
        }

        public async Task<UseCaseResult<int>> ExecuteAsync( string adminUserId, ConcederPermissaoAdminTemporariaDTO dto)
        {
            var funcionario = await _userManager.FindByIdAsync(dto.FuncionarioUserId);

            if (funcionario == null)
            {
                return UseCaseResult<int>.Falha("Funcionário não encontrado.", TipoErro.NaoEncontrado);
            }

            // A permissão temporária é exclusiva para Funcionários.
            var eFuncionario = await _userManager.IsInRoleAsync(funcionario, "Funcionario");

            if (!eFuncionario)
            {
                return UseCaseResult<int>.Falha("A permissão administrativa temporária só pode ser concedida a um Funcionário.");
            }

            var agora = DateTime.UtcNow;

            // Verifica se já existe uma permissão válida.
            var permissaoAtiva = await _permissaoRepository
                .GetAllWithUsers()
                .AnyAsync(p =>
                    p.FuncionarioUserId == funcionario.Id &&
                    !p.Revogada &&
                    p.DataInicio <= agora &&
                    p.DataFim > agora);

            if (permissaoAtiva)
            {
                return UseCaseResult<int>.Falha("Este funcionário já possui uma permissão administrativa temporária ativa.");
            }

            var permissao = new PermissaoAdminTemporaria
            {
                FuncionarioUserId = funcionario.Id,
                ConcedidoPorUserId = adminUserId,

                DataInicio = agora,
                DataFim = agora.AddMinutes(dto.DuracaoMinutos),

                Motivo = string.IsNullOrWhiteSpace(dto.Motivo)
                    ? null
                    : dto.Motivo.Trim(),

                Revogada = false,
                DataRevogacao = null,
                DataCriacao = agora
            };

            await _permissaoRepository.CreateAsync(permissao);

            await _notificacaoService.CriarNotificacaoAsync(
                funcionario.Id,
                "Privilégios administrativos temporários",
                $"O administrador concedeu-lhe privilégios administrativos " +
                $"durante {dto.DuracaoMinutos} minutos.");

            return UseCaseResult<int>.Ok(permissao.Id);
        }
    }
}
