using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

using ProjetoFinalCet105.Mobile.Services;
using ProjetoFinalCet105.Mobile.Views;

namespace ProjetoFinalCet105.Mobile.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;

    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _mensagemErro = string.Empty;
    private bool _isBusy;

    public LoginViewModel()
    {
        _apiService = new ApiService();
        _authService = new AuthService();

        LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
        }
    }

    public string MensagemErro
    {
        get => _mensagemErro;
        set
        {
            _mensagemErro = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TemErro));
        }
    }

    public bool TemErro => !string.IsNullOrWhiteSpace(MensagemErro);

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            ((Command)LoginCommand).ChangeCanExecute();
        }
    }

    public ICommand LoginCommand { get; }

    private async Task LoginAsync()
    {
        MensagemErro = string.Empty;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            MensagemErro = "Introduza o email e a palavra-passe.";
            return;
        }

        try
        {
            IsBusy = true;

            var resultado = await _apiService.LoginAsync(Email.Trim(), Password);

            if (resultado == null)
            {
                MensagemErro = "Email ou palavra-passe inválidos.";
                return;
            }

            if (resultado.RequiresTwoFactor)
            {
                MensagemErro = "A autenticação de dois fatores ainda não está disponível na aplicação móvel.";
                return;
            }

            if (!resultado.Roles.Any(r => r.Equals("Cliente", StringComparison.OrdinalIgnoreCase)))
            {
                MensagemErro = "A aplicação móvel está disponível apenas para clientes.";
                return;
            }

            if (string.IsNullOrWhiteSpace(resultado.Token))
            {
                MensagemErro = "Não foi possível iniciar sessão.";
                return;
            }

            await _authService.GuardarSessaoAsync(resultado.Token, resultado.UserId, resultado.NomeCompleto, resultado.Email);


            await Shell.Current.GoToAsync(nameof(ClienteHomePage));
        }
        catch
        {
            MensagemErro = "Não foi possível comunicar com o servidor.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}