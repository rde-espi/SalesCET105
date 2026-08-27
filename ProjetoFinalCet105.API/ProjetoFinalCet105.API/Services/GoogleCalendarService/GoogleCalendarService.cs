using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.DataProtection;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Services.GoogleCalendarService
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly IConfiguration _configuration;
        private readonly IDataProtector _stateProtector;
        private const string RedirectUri ="https://localhost:44349/api/GoogleCalendar/callback";

        public GoogleCalendarService(IConfiguration configuration, IDataProtectionProvider dataProtectionProvider)
        {
            _configuration = configuration;
            _stateProtector = dataProtectionProvider.CreateProtector("GoogleCalendar.OAuth.State");
        }

        private GoogleAuthorizationCodeFlow CriarFlow()
        {
            var clientId =_configuration["GoogleAuth:ClientId"];

            var clientSecret = _configuration["GoogleAuth:ClientSecret"];

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new InvalidOperationException("As credenciais Google não estão configuradas.");
            }

            return new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    },

                    Scopes = new[]
                    {
                        CalendarService.Scope.CalendarEvents
                    }
                });
        }

        public string GerarUrlAutorizacao(string userId)
        {
            var flow = CriarFlow();

            var request = flow.CreateAuthorizationCodeRequest(RedirectUri);

            var stateProtegido = _stateProtector.Protect(userId);

            request.State = stateProtegido;

            return request.Build().ToString();
        }

        public async Task<string?> TrocarCodigoPorRefreshTokenAsync(string code,CancellationToken cancellationToken = default)
        {
            var flow = CriarFlow();

            var token = await flow.ExchangeCodeForTokenAsync(
                userId: "google-calendar",
                code: code,
                redirectUri: RedirectUri,
                taskCancellationToken: cancellationToken);

            return token.RefreshToken;
        }

        public string? ObterUserIdDoState(string state)
        {
            if (string.IsNullOrWhiteSpace(state))
                return null;

            try
            {
                return _stateProtector.Unprotect(state);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> CriarEventoAsync(
            GoogleCalendarConta conta,
            string titulo,
            string descricao,
            DateTime inicio,
            DateTime fim,
            CancellationToken cancellationToken = default)
        {
            var clientId = _configuration["GoogleAuth:ClientId"];

            var clientSecret = _configuration["GoogleAuth:ClientSecret"];

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new InvalidOperationException( "As credenciais Google não estão configuradas.");
            }

            var flow = CriarFlow();

            var token = new TokenResponse
            {
                RefreshToken = conta.RefreshToken
            };

            var credential = new UserCredential( flow, conta.UserId, token);

            var accessToken = await credential.GetAccessTokenForRequestAsync( cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException( "Não foi possível obter um access token Google.");
            }

            var calendarService =
                new CalendarService(
                    new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "ProjetoFinalCet105"
                    });

            var evento = new Event
            {
                Summary = titulo,
                Description = descricao,

                Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(inicio),
                    TimeZone = "Europe/Lisbon"
                },

                End = new EventDateTime
                {
                    DateTimeDateTimeOffset = new DateTimeOffset(fim),
                    TimeZone = "Europe/Lisbon"
                }
            };

            var request = calendarService.Events.Insert( evento, conta.CalendarId);

            var criado = await request.ExecuteAsync( cancellationToken);

            if (string.IsNullOrWhiteSpace(criado.Id))
            {
                throw new InvalidOperationException( "O Google Calendar não devolveu o ID do evento.");
            }

            return criado.Id;
        }

        public async Task AtualizarEventoAsync(
            GoogleCalendarConta conta,
            string googleEventId,
            string titulo,
            string descricao,
            DateTime inicio,
            DateTime fim,
            CancellationToken cancellationToken = default)
        {
            var flow = CriarFlow();

            var token = new TokenResponse
            {
                RefreshToken = conta.RefreshToken
            };

            var credential = new UserCredential(flow, conta.UserId, token);

            var calendarService =
                new CalendarService(
                    new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "ProjetoFinalCet105"
                    });

            var evento = await calendarService.Events
                .Get(conta.CalendarId, googleEventId)
                .ExecuteAsync(cancellationToken);

            evento.Summary = titulo;
            evento.Description = descricao;

            evento.Start = new EventDateTime
            {
                DateTimeDateTimeOffset =
                    new DateTimeOffset(inicio),
                TimeZone = "Europe/Lisbon"
            };

            evento.End = new EventDateTime
            {
                DateTimeDateTimeOffset =
                    new DateTimeOffset(fim),
                TimeZone = "Europe/Lisbon"
            };

            await calendarService.Events
                .Update(
                evento,
                conta.CalendarId,
                googleEventId)
                .ExecuteAsync(cancellationToken);
        }

        public async Task EliminarEventoAsync( GoogleCalendarConta conta,string googleEventId, CancellationToken cancellationToken = default)
        {
            var flow = CriarFlow();

            var token = new TokenResponse
            {
                RefreshToken = conta.RefreshToken
            };

            var credential = new UserCredential(flow, conta.UserId, token);

            var calendarService =
                new CalendarService(
                    new BaseClientService.Initializer
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "ProjetoFinalCet105"
                    });

            await calendarService.Events
                .Delete(
                conta.CalendarId,
                googleEventId)
                .ExecuteAsync(cancellationToken);
        }
    }
}
