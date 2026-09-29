using ProjetoFinalCet105.Mobile.ViewModels;

namespace ProjetoFinalCet105.Mobile.Views;

[QueryProperty(nameof(MarcacaoId), "marcacaoId")]
public partial class ReagendarMarcacaoPage : ContentPage
{
    private readonly ReagendarMarcacaoViewModel _viewModel;
    private int _marcacaoId;
    private bool _carregada;

    public int MarcacaoId
    {
        get => _marcacaoId;
        set => _marcacaoId = value;
    }

    public ReagendarMarcacaoPage()
    {
        InitializeComponent();

        _viewModel = new ReagendarMarcacaoViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_carregada || MarcacaoId <= 0)
            return;

        _carregada = true;

        await _viewModel.CarregarMarcacaoAsync(MarcacaoId);
    }

    private async void Voltar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void VerMarcacoes_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}