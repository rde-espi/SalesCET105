using FirebaseAdmin.Messaging;

namespace ProjetoFinalCet105.API.Services.FirebaseService
{
    public class FirebaseService : IFirebaseService
    {
        public async Task<string> EnviarPushAsync(string fid,string titulo,string mensagem)
        {
            var message = new Message
            {
                Fid = fid,

                Notification = new Notification
                {
                    Title = titulo,
                    Body = mensagem
                }
            };

            var resposta = await FirebaseMessaging.DefaultInstance
                    .SendAsync(message);

            return resposta;
        }
    }
}
