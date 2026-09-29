using ProjetoFinalCet105.Mobile.ViewModels;

namespace ProjetoFinalCet105.Mobile.Views;

public partial class NovaMarcacaoPage : ContentPage
{
    private readonly NovaMarcacaoViewModel _viewModel;

    public NovaMarcacaoPage()
    {
        InitializeComponent();

        _viewModel = new NovaMarcacaoViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.Servicos.Count == 0)
            await _viewModel.CarregarServicosAsync();
    }

    private async void Voltar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void VerMarcacoes_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MarcacoesPage));
    }
}