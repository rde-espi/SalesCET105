namespace ProjetoFinalCet105.API.Services.FirebaseService
{
    public interface IFirebaseService
    {
        Task<string> EnviarPushAsync(string fid,string titulo,string mensagem);
    }
}
