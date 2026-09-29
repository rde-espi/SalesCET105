using ProjetoFinalCet105.Mobile.Models;
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

    private async void Cancelar_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not Marcacao marcacao)
            return;

        var confirmar = await DisplayAlertAsync(
            "Cancelar marcação",
            $"Tem a certeza de que pretende cancelar a marcação de {marcacao.ServicoNome}?",
            "Sim, cancelar",
            "Não");

        if (!confirmar)
            return;

        var sucesso = await _viewModel.CancelarMarcacaoAsync(marcacao.Id);

        if (sucesso)
        {
            await DisplayAlertAsync(
                "Marcação cancelada",
                "A marcação foi cancelada com sucesso.",
                "OK");
        }
    }

    private async void Reagendar_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not Marcacao marcacao)
            return;

        await DisplayAlertAsync(
            "Reagendar marcação",
            $"O reagendamento de {marcacao.ServicoNome} será disponibilizado de seguida.",
            "OK");
    }
}