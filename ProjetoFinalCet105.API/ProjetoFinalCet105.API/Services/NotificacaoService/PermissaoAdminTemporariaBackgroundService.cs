using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.Services.NotificacaoService
{
    public class PermissaoAdminTemporariaBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PermissaoAdminTemporariaBackgroundService> _logger;

        public PermissaoAdminTemporariaBackgroundService( IServiceScopeFactory scopeFactory, ILogger<PermissaoAdminTemporariaBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var permissaoRepository = scope.ServiceProvider.GetRequiredService<IPermissaoAdminTemporariaRepository>();

                    var notificacaoService = scope.ServiceProvider.GetRequiredService<INotificacaoService>();

                    var agora = DateTime.UtcNow;

                    var permissoesExpiradas = await permissaoRepository
                        .GetAllWithUsers()
                        .Where(p =>
                            !p.Revogada &&
                            p.DataFim <= agora &&
                            p.DataNotificacaoExpiracao == null)
                        .ToListAsync(stoppingToken);

                    foreach (var permissao in permissoesExpiradas)
                    {
                        try
                        {
                            await notificacaoService.CriarNotificacaoAsync(
                                permissao.FuncionarioUserId,
                                "Privilégios administrativos expirados",
                                "Os seus privilégios administrativos temporários expiraram. " +
                                "O seu perfil voltou automaticamente ao nível de Funcionário.");

                            permissao.DataNotificacaoExpiracao = agora;

                            // Evita o conflito de tracking causado pelas navegações User
                            permissao.FuncionarioUser = null!;
                            permissao.ConcedidoPorUser = null!;

                            await permissaoRepository.UpdateAsync(permissao);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(
                                ex,
                                "Falha ao processar a expiração da permissão administrativa temporária {PermissaoId}.",
                                permissao.Id);
                        }
                    }
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro durante o processamento das permissões administrativas temporárias expiradas.");
                }

                try
                {
                    await Task.Delay( TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
