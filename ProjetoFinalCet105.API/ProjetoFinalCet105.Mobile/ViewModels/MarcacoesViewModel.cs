using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

using ProjetoFinalCet105.Mobile.Models;

using ProjetoFinalCet105.Mobile.Services;

namespace ProjetoFinalCet105.Mobile.ViewModels;

public class MarcacoesViewModel : INotifyPropertyChanged
{
    private readonly ApiService _apiService;

    private bool _isBusy;
    private string _mensagemErro = string.Empty;

    public MarcacoesViewModel()
    {
        _apiService = new ApiService();

        Marcacoes = new ObservableCollection<Marcacao>();

        CarregarCommand = new Command( async () => await CarregarAsync(), () => !IsBusy);
    }

    public ObservableCollection<Marcacao> Marcacoes { get; }
    public ObservableCollection<Marcacao> ProximasMarcacoes { get; } = new ObservableCollection<Marcacao>();

    public ObservableCollection<Marcacao> HistoricoMarcacoes { get; } = new ObservableCollection<Marcacao>();

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(NaoEstaOcupado));

            ((Command)CarregarCommand).ChangeCanExecute();
        }
    }

    public bool NaoEstaOcupado => !IsBusy;

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

    public bool SemMarcacoes => !IsBusy && !TemErro && Marcacoes.Count == 0;

    public ICommand CarregarCommand { get; }

    public async Task CarregarAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            MensagemErro = string.Empty;

            var resultado = await _apiService.GetAsync<List<Marcacao>>( "api/Marcacoes");

            Marcacoes.Clear();
            ProximasMarcacoes.Clear();
            HistoricoMarcacoes.Clear();

            if (resultado != null)
            {
                var agora = DateTime.Now;

                foreach (var marcacao in resultado)
                {
                    Marcacoes.Add(marcacao);

                    if (marcacao.DataHoraInicio >= agora && !string.Equals( marcacao.EstadoMarcacaoNome, "Cancelada",  StringComparison.OrdinalIgnoreCase))
                    {
                        ProximasMarcacoes.Add(marcacao);
                    }
                    else
                    {
                        HistoricoMarcacoes.Add(marcacao);
                    }
                }

                var proximasOrdenadas = ProximasMarcacoes
                    .OrderBy(m => m.DataHoraInicio)
                    .ToList();

                ProximasMarcacoes.Clear();

                foreach (var marcacao in proximasOrdenadas)
                {
                    ProximasMarcacoes.Add(marcacao);
                }
                    


                var historicoOrdenado = HistoricoMarcacoes
                    .OrderByDescending(m => m.DataHoraInicio)
                    .ToList();

                HistoricoMarcacoes.Clear();

                foreach (var marcacao in historicoOrdenado)
                {
                    HistoricoMarcacoes.Add(marcacao);
                }
                    
            }

            OnPropertyChanged(nameof(SemMarcacoes));
        }
        catch
        {
            Marcacoes.Clear();

            MensagemErro = "Não foi possível carregar as suas marcações.";

            OnPropertyChanged(nameof(SemMarcacoes));
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(SemMarcacoes));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged( [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs(propertyName));
    }
}   