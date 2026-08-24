using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.EmailService;

namespace ProjetoFinalCet105.API.Services
{
    public class LembreteMarcacoesBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public LembreteMarcacoesBackgroundService(
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var marcacaoRepository =
                    scope.ServiceProvider
                        .GetRequiredService<IMarcacaoRepository>();

                var emailService =
                    scope.ServiceProvider
                        .GetRequiredService<IEmailService>();

                var agora = DateTime.Now;

                var limiteInicio =
                    agora.AddHours(23).AddMinutes(50);

                var limiteFim =
                    agora.AddHours(24).AddMinutes(10);

                var marcacoes =
                    await marcacaoRepository
                        .GetAllWithDetails()
                        .Where(m =>
                            !m.Lembrete24hEnviado &&
                            m.DataHoraInicio >= limiteInicio &&
                            m.DataHoraInicio <= limiteFim &&
                            m.EstadoMarcacao.Nome == "Confirmada")
                        .ToListAsync(stoppingToken);

                foreach (var marcacao in marcacoes)
                {
                    try
                    {
                        var emailCliente =
                            marcacao.Cliente.Email;

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

                        await emailService.EnviarEmailAsync(
                            emailCliente,
                            "Lembrete da sua marcação",
                            mensagem);

                        marcacao.Lembrete24hEnviado = true;

                        await marcacaoRepository
                            .UpdateAsync(marcacao);
                    }
                    catch
                    {
                        // Se falhar, mantém false para tentar novamente
                        // numa próxima execução.
                    }
                }

                await Task.Delay(
                    TimeSpan.FromMinutes(10),
                    stoppingToken);
            }
        }
    }
}
