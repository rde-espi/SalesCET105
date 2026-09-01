using FirebaseAdmin;
using FirebaseAdmin.Messaging;

namespace ProjetoFinalCet105.API.Services.FirebaseService
{
    public class FirebaseService : IFirebaseService
    {
        private readonly ILogger<FirebaseService> _logger;

        public FirebaseService(ILogger<FirebaseService> logger)
        {
            _logger = logger;
        }

        public async Task<string> EnviarPushAsync(
            string fid,
            string titulo,
            string mensagem)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                _logger.LogWarning("Tentativa de envio de notificação Firebase, mas o Firebase não está inicializado.");

                throw new InvalidOperationException("O Firebase não está inicializado.");
            }

            var message = new Message
            {
                Fid = fid,

                Notification = new Notification
                {
                    Title = titulo,
                    Body = mensagem
                }
            };

            var resposta = await FirebaseMessaging.DefaultInstance.SendAsync(message);

            return resposta;
        }
    }
}
