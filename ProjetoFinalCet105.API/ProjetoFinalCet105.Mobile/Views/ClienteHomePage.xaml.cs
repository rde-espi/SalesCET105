namespace ProjetoFinalCet105.Mobile.Views;

public partial class ClienteHomePage : ContentPage
{
	public ClienteHomePage()
	{
		InitializeComponent();
	}


    private async void Marcacoes_Tapped( object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync( nameof(MarcacoesPage));
    }

    private async void NovaMarcacao_Tapped( object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync( nameof(NovaMarcacaoPage));
    }
}