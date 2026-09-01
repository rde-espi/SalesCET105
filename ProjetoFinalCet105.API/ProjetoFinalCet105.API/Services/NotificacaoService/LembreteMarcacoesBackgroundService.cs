using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.EmailService;
using ProjetoFinalCet105.API.Services.FirebaseService;

namespace ProjetoFinalCet105.API.Services.NotificacaoService
{
    public class LembreteMarcacoesBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LembreteMarcacoesBackgroundService> _logger;

        public LembreteMarcacoesBackgroundService(IServiceScopeFactory scopeFactory, ILogger<LembreteMarcacoesBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var marcacaoRepository = scope.ServiceProvider.GetRequiredService<IMarcacaoRepository>();

                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var firebaseService = scope.ServiceProvider.GetRequiredService<IFirebaseService>();

                    var dispositivoRepository = scope.ServiceProvider.GetRequiredService<IDispositivoUserRepository>();

                    var agora = DateTime.Now;

                    var limiteInicio = agora.AddHours(23).AddMinutes(50);

                    var limiteFim = agora.AddHours(24).AddMinutes(10);

                    var marcacoes = await marcacaoRepository
                        .GetAllWithDetails()
                        .Where(m =>
                        !m.Lembrete24hEnviado &&
                        m.DataHoraInicio >= limiteInicio &&
                        m.DataHoraInicio <= limiteFim &&
                        m.EstadoMarcacao.Nome == "Confirmada")
                        .ToListAsync(stoppingToken);

                    var limite30Inicio = agora.AddMinutes(25);

                    var limite30Fim = agora.AddMinutes(35);

                    var marcacoes30Min = await marcacaoRepository
                        .GetAllWithDetails()
                        .Where(m =>
                        !m.Lembrete30MinEnviado &&
                        m.DataHoraInicio >= limite30Inicio &&
                        m.DataHoraInicio <= limite30Fim &&
                        m.EstadoMarcacao.Nome == "Confirmada")
                        .ToListAsync(stoppingToken);

                    foreach (var marcacao in marcacoes30Min)
                    {
                        try
                        {
                            var dispositivos = await dispositivoRepository
                                .GetAtivosByUserId(marcacao.ClienteId)
                                .ToListAsync(stoppingToken);

                            if (!dispositivos.Any())
                            {
                                continue;
                            }

                            foreach (var dispositivo in dispositivos)
                            {
                                await firebaseService.EnviarPushAsync(
                                    dispositivo.Fid,
                                    "A sua marcação está próxima",
                                    $"A sua marcação de {marcacao.Servico.Nome} " +
                                    $"começa às {marcacao.DataHoraInicio:HH:mm}.");
                            }

                            marcacao.Lembrete30MinEnviado = true;

                            await marcacaoRepository.UpdateAsync(marcacao);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning( ex,"Falha ao enviar lembrete de 30 minutos da marcação {MarcacaoId}. Será tentado novamente.", marcacao.Id);
                        }
                    }

                    foreach (var marcacao in marcacoes)
                    {
                        try
                        {
                            var emailCliente = marcacao.Cliente.Email;

                            if (string.IsNullOrWhiteSpace(emailCliente))
                            {
                                continue;
                            }

                            var mensagem = $@"
                            <h2>Lembrete de marcação</h2>

                            <p>Olá {marcacao.Cliente.NomeCompleto},</p>

                            <p>Recordamos que tem uma marcação amanhã.</p>

                            <p>
                                <strong>Serviço:</strong>
                                {marcacao.Servico.Nome}
                            </p>

                            <p>
                                <strong>Data:</strong>
                                {marcacao.DataHoraInicio:dd/MM/yyyy}
                            </p>

                            <p>
                                <strong>Hora:</strong>
                                {marcacao.DataHoraInicio:HH:mm}
                            </p>

                            <p>
                                <strong>Profissional:</strong>
                                {marcacao.Funcionario.User.NomeCompleto}
                            </p>";

                            await emailService.EnviarEmailAsync(emailCliente, "Lembrete da sua marcação", mensagem);

                            marcacao.Lembrete24hEnviado = true;

                            await marcacaoRepository.UpdateAsync(marcacao);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,"Falha ao enviar lembrete de 24 horas da marcação {MarcacaoId}. Será tentado novamente.", marcacao.Id);
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
                    _logger.LogError( ex,"Erro durante o processamento dos lembretes de marcações.");
                }

                try
                {
                    await Task.Delay( TimeSpan.FromMinutes(10), stoppingToken);
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
