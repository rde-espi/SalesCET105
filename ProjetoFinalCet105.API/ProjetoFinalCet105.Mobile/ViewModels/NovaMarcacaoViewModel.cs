using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

using ProjetoFinalCet105.Mobile.Models;
using ProjetoFinalCet105.Mobile.Services;

namespace ProjetoFinalCet105.Mobile.ViewModels;

public class NovaMarcacaoViewModel : INotifyPropertyChanged
{
    private readonly ApiService _apiService;

    private bool _isBusy;
    private string _mensagemErro = string.Empty;
    private Servico? _servicoSelecionado;
    private DateTime _dataSelecionada = DateTime.Today;
    private DateTime? _horarioSelecionado;
    private bool _aCarregarHorarios;
    private string _observacoes = string.Empty;
    private bool _aGuardar;
    private string _mensagemSucesso = string.Empty;
    private bool _marcacaoConcluida;


    public NovaMarcacaoViewModel()
    {
        _apiService = new ApiService();

        Servicos = new ObservableCollection<Servico>();
        ConfirmarMarcacaoCommand = new Command( async () => await ConfirmarMarcacaoAsync(), () => PodeConfirmar);
    }



    public bool MarcacaoConcluida
    {
        get => _marcacaoConcluida;
        private set
        {
            if (_marcacaoConcluida == value)
                return;

            _marcacaoConcluida = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(PodeConfirmar));

            ConfirmarMarcacaoCommand.ChangeCanExecute();
        }
    }

    public string Observacoes
    {
        get => _observacoes;
        set
        {
            if (_observacoes == value)
                return;

            _observacoes = value;
            OnPropertyChanged();
        }
    }

    public bool AGuardar
    {
        get => _aGuardar;
        set
        {
            if (_aGuardar == value)
                return;

            _aGuardar = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PodeConfirmar));
            ConfirmarMarcacaoCommand.ChangeCanExecute();
        }
    }

    public string MensagemSucesso
    {
        get => _mensagemSucesso;
        set
        {
            if (_mensagemSucesso == value)
                return;

            _mensagemSucesso = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TemSucesso));
        }
    }

    public bool TemSucesso => !string.IsNullOrWhiteSpace(MensagemSucesso);

    public bool PodeConfirmar =>
      ServicoSelecionado != null &&
      FuncionarioSelecionado != null &&
      HorarioSelecionado.HasValue &&
      !AGuardar &&
      !MarcacaoConcluida;


    public ObservableCollection<Servico> Servicos { get; }

    public Servico? ServicoSelecionado
    {
        get => _servicoSelecionado;
        set
        {
            if (_servicoSelecionado == value)
                return;

            _servicoSelecionado = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(TemServicoSelecionado));

            FuncionarioSelecionado = null;
            Funcionarios.Clear();

            if (_servicoSelecionado != null)
            {
                _ = CarregarFuncionariosAsync(
                    _servicoSelecionado.Id);
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value)
                return;

            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public string MensagemErro
    {
        get => _mensagemErro;
        set
        {
            if (_mensagemErro == value)
                return;

            _mensagemErro = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TemErro));
        }
    }

    public DateTime DataSelecionada
    {
        get => _dataSelecionada;
        set
        {
            if (_dataSelecionada.Date == value.Date)
                return;

            _dataSelecionada = value.Date;

            HorarioSelecionado = null;
            HorariosDisponiveis.Clear();

            OnPropertyChanged();
            OnPropertyChanged(nameof(TemHorariosDisponiveis));

            if (FuncionarioSelecionado != null &&
                ServicoSelecionado != null)
            {
                _ = CarregarHorariosAsync();
            }
        }
    }

    public DateTime DataMinima => DateTime.Today;

    public bool TemFuncionarioSelecionado => FuncionarioSelecionado != null;

    public bool TemErro => !string.IsNullOrWhiteSpace(MensagemErro);

    public async Task CarregarServicosAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            MensagemErro = string.Empty;

            var resultado = await _apiService.GetAsync<List<Servico>>( "api/Servicos");

            Servicos.Clear();

            if (resultado == null)
            {
                MensagemErro = "Não foi possível carregar os serviços.";

                return;
            }

            foreach (var servico in resultado.Where(s => s.Disponivel).OrderBy(s => s.Nome))
            {
                Servicos.Add(servico);
            }
        }
        catch
        {
            MensagemErro = "Não foi possível carregar os serviços.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public Command ConfirmarMarcacaoCommand { get; }

    private Funcionario? _funcionarioSelecionado;
    private bool _aCarregarFuncionarios;

    public ObservableCollection<Funcionario> Funcionarios { get; } = new ObservableCollection<Funcionario>();

    public Funcionario? FuncionarioSelecionado
    {
        get => _funcionarioSelecionado;
        set
        {
            if (_funcionarioSelecionado == value)
                return;

            _funcionarioSelecionado = value;

            HorarioSelecionado = null;
            HorariosDisponiveis.Clear();

            OnPropertyChanged();
            OnPropertyChanged(nameof(TemFuncionarioSelecionado));
            OnPropertyChanged(nameof(TemHorariosDisponiveis));

            if (_funcionarioSelecionado != null &&
                ServicoSelecionado != null)
            {
                _ = CarregarHorariosAsync();
            }
        }
    }

    public bool TemServicoSelecionado => ServicoSelecionado != null;

    public bool ACarregarFuncionarios
    {
        get => _aCarregarFuncionarios;
        set
        {
            if (_aCarregarFuncionarios == value)
                return;

            _aCarregarFuncionarios = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DateTime> HorariosDisponiveis { get; } = new ObservableCollection<DateTime>();

    public DateTime? HorarioSelecionado
    {
        get => _horarioSelecionado;
        set
        {
            if (_horarioSelecionado == value)
                return;

            _horarioSelecionado = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PodeConfirmar));
            ConfirmarMarcacaoCommand.ChangeCanExecute();
        }
    }

    public bool ACarregarHorarios
    {
        get => _aCarregarHorarios;
        set
        {
            if (_aCarregarHorarios == value)
                return;

            _aCarregarHorarios = value;
            OnPropertyChanged();
        }
    }

    public bool TemHorariosDisponiveis => HorariosDisponiveis.Count > 0;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged( [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs(propertyName));
    }

    private async Task CarregarFuncionariosAsync( int servicoId)
    {
        try
        {
            ACarregarFuncionarios = true;
            MensagemErro = string.Empty;

            var resultado = await _apiService.GetAsync<List<Funcionario>>( $"api/Funcionarios/servico/{servicoId}");

            Funcionarios.Clear();

            if (resultado == null)
            {
                MensagemErro = "Não foi possível carregar os profissionais.";

                return;
            }

            foreach (var funcionario in resultado.Where(f => f.Disponivel).OrderBy(f => f.NomeCompleto))
            {
                Funcionarios.Add(funcionario);
            }
        }
        catch
        {
            Funcionarios.Clear();

            MensagemErro = "Não foi possível carregar os profissionais.";
        }
        finally
        {
            ACarregarFuncionarios = false;
        }
    }

    private async Task CarregarHorariosAsync()
    {
        if (FuncionarioSelecionado == null || ServicoSelecionado == null)
            return;

        try
        {
            ACarregarHorarios = true;
            MensagemErro = string.Empty;

            HorarioSelecionado = null;
            HorariosDisponiveis.Clear();

            var endpoint =
                $"api/Marcacoes/disponibilidade" +
                $"?funcionarioId={FuncionarioSelecionado.Id}" +
                $"&servicoId={ServicoSelecionado.Id}" +
                $"&data={DataSelecionada:yyyy-MM-dd}";

            var resultado = await _apiService.GetAsync<List<DateTime>>(endpoint);

            if (resultado != null)
            {
                foreach (var horario in resultado.OrderBy(h => h))
                {
                    HorariosDisponiveis.Add(horario);
                }
            }

            OnPropertyChanged(nameof(TemHorariosDisponiveis));
        }
        catch
        {
            HorariosDisponiveis.Clear();

            MensagemErro = "Não foi possível consultar os horários disponíveis.";

            OnPropertyChanged(nameof(TemHorariosDisponiveis));
        }
        finally
        {
            ACarregarHorarios = false;
        }
    }

    private async Task ConfirmarMarcacaoAsync()
    {
        if (!PodeConfirmar ||
            ServicoSelecionado == null ||
            FuncionarioSelecionado == null ||
            !HorarioSelecionado.HasValue)
            return;

        try
        {
            AGuardar = true;
            MensagemErro = string.Empty;
            MensagemSucesso = string.Empty;

            var request = new NovaMarcacaoRequest
            {
                ServicoId = ServicoSelecionado.Id,
                FuncionarioId = FuncionarioSelecionado.Id,
                DataHoraInicio = HorarioSelecionado.Value,
                Observacoes = string.IsNullOrWhiteSpace(Observacoes)
                    ? null
                    : Observacoes.Trim(),
                PromoCode = null
            };

            var resultado = await _apiService.PostAsync<NovaMarcacaoRequest, Marcacao>( "api/Marcacoes", request);

            if (resultado == null)
            {
                MensagemErro = "Não foi possível efetuar a marcação.";

                return;
            }

            MensagemSucesso = "Marcação efetuada com sucesso.";
        }
        catch
        {
            MensagemErro = "Não foi possível efetuar a marcação.";
        }
        finally
        {
            AGuardar = false;
        }
    }
}
