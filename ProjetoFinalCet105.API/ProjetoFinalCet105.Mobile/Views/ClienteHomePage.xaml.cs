using ProjetoFinalCet105.Mobile.Services;

namespace ProjetoFinalCet105.Mobile.Views;

public partial class ClienteHomePage : ContentPage
{
    public ClienteHomePage()
    {
        InitializeComponent();
    }


    private async void Marcacoes_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MarcacoesPage));
    }

    private async void NovaMarcacao_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NovaMarcacaoPage));
    }

    private async void Logout_Clicked(object sender, EventArgs e)
    {
        var confirmar = await DisplayAlertAsync(
            "Terminar sessão",
            "Pretende terminar a sessão?",
            "Sim",
            "Não");

        if (!confirmar)
            return;

        var authService = new AuthService();
        authService.Logout();

        await Shell.Current.GoToAsync("//LoginPage");
    }
}