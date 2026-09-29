using Microsoft.Extensions.DependencyInjection;

using ProjetoFinalCet105.Mobile.Services;
using ProjetoFinalCet105.Mobile.Views;

namespace ProjetoFinalCet105.Mobile;

public partial class App : Application
{
    private readonly AuthService _authService;

    public App()
    {
        InitializeComponent();

        _authService = new AuthService();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (await _authService.EstaAutenticadoAsync())
            {
                await Shell.Current.GoToAsync(nameof(ClienteHomePage));
            }
        });

        return window;
    }
}