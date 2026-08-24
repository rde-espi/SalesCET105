namespace ProjetoFinalCet105.API.Services.EmailService
{
    public interface IEmailService
    {
        Task EnviarEmailAsync(string destinatario,string assunto,string mensagem);
    }
}
