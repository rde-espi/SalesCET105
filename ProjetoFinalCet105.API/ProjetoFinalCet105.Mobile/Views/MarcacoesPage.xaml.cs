using ProjetoFinalCet105.Mobile.ViewModels;

namespace ProjetoFinalCet105.Mobile.Views;

public partial class MarcacoesPage : ContentPage
{
    private readonly MarcacoesViewModel _viewModel;

    public MarcacoesPage()
    {
        InitializeComponent();

        _viewModel = new MarcacoesViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.CarregarAsync();
    }

    private async void Voltar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}