using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

using ProjetoFinalCet105.Mobile.Models;
using ProjetoFinalCet105.Mobile.Services;

namespace ProjetoFinalCet105.Mobile.ViewModels
{
    public class ReagendarMarcacaoViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService = new();

        private Marcacao? _marcacao;
        private DateTime _dataSelecionada = DateTime.Today;
        private DateTime? _horarioSelecionado;
        private bool _isBusy;
        private string _mensagemErro = string.Empty;
        private bool _aReagendar;
        private bool _reagendamentoConcluido;
        private DataDisponivel? _dataDisponivelSelecionada;
        private bool _aCarregarDatas;

        public ReagendarMarcacaoViewModel()
        {
            ConfirmarReagendamentoCommand = new Command(
                async () => await ConfirmarReagendamentoAsync(),
                () => PodeConfirmar);
        }

        public ObservableCollection<DateTime> HorariosDisponiveis { get; } = new();
        public ObservableCollection<DataDisponivel> DatasDisponiveis { get; } = new();

        public DataDisponivel? DataDisponivelSelecionada
        {
            get => _dataDisponivelSelecionada;
            set
            {
                if (_dataDisponivelSelecionada == value)
                    return;

                _dataDisponivelSelecionada = value;
                OnPropertyChanged();

                HorarioSelecionado = null;
                HorariosDisponiveis.Clear();

                if (_dataDisponivelSelecionada != null)
                {
                    DataSelecionada = _dataDisponivelSelecionada.Data;
                }
            }
        }

        public bool ACarregarDatas
        {
            get => _aCarregarDatas;
            private set
            {
                if (_aCarregarDatas == value)
                    return;

                _aCarregarDatas = value;
                OnPropertyChanged();
            }
        }

        public bool TemDatasDisponiveis =>
            DatasDisponiveis.Count > 0;

        public Marcacao? Marcacao
        {
            get => _marcacao;
            private set
            {
                _marcacao = value;
                OnPropertyChanged();
            }
        }

        public DateTime DataSelecionada
        {
            get => _dataSelecionada;
            set
            {
                if (_dataSelecionada.Date == value)
                    return;

                _dataSelecionada = value.Date;
                OnPropertyChanged();

                HorarioSelecionado = null;
                HorariosDisponiveis.Clear();

                if (Marcacao != null)
                    _ = CarregarHorariosAsync();
            }
        }

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

                ConfirmarReagendamentoCommand.ChangeCanExecute();
            }
        }

        public DateTime DataMinima => DateTime.Today;

        public bool IsBusy
        {
            get => _isBusy;
            private set
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
            private set
            {
                _mensagemErro = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TemErro));
            }
        }

        public bool TemErro => !string.IsNullOrWhiteSpace(MensagemErro);

        public async Task CarregarMarcacaoAsync(int marcacaoId)
        {
            try
            {
                IsBusy = true;
                MensagemErro = string.Empty;

                Marcacao = await _apiService.GetAsync<Marcacao>($"api/Marcacoes/{marcacaoId}");

                if (Marcacao == null)
                {
                    MensagemErro = "Não foi possível carregar a marcação.";
                    return;
                }

                await CarregarDatasDisponiveisAsync();
            }
            catch
            {
                MensagemErro = "Não foi possível carregar a marcação.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CarregarDatasDisponiveisAsync()
        {
            if (Marcacao == null)
                return;

            try
            {
                ACarregarDatas = true;
                MensagemErro = string.Empty;

                DatasDisponiveis.Clear();
                DataDisponivelSelecionada = null;

                HorarioSelecionado = null;
                HorariosDisponiveis.Clear();

                OnPropertyChanged(nameof(TemDatasDisponiveis));

                var data = DateTime.Today;

                const int limiteDias = 30;
                const int quantidadeDatas = 7;

                for (var i = 0; i < limiteDias && DatasDisponiveis.Count < quantidadeDatas; i++)
                {
                    var dataConsulta = data.AddDays(i);

                    var endpoint =
                        $"api/Marcacoes/disponibilidade" +
                        $"?funcionarioId={Marcacao.FuncionarioId}" +
                        $"&servicoId={Marcacao.ServicoId}" +
                        $"&data={dataConsulta:yyyy-MM-dd}";

                    var horarios = await _apiService.GetAsync<List<DateTime>>(endpoint);

                    if (horarios != null && horarios.Count > 0)
                    {
                        DatasDisponiveis.Add(new DataDisponivel
                        {
                            Data = dataConsulta,
                            QuantidadeHorarios = horarios.Count
                        });
                    }
                }

                OnPropertyChanged(nameof(TemDatasDisponiveis));
            }
            catch
            {
                DatasDisponiveis.Clear();

                MensagemErro = "Não foi possível consultar as datas disponíveis.";

                OnPropertyChanged(nameof(TemDatasDisponiveis));
            }
            finally
            {
                ACarregarDatas = false;
            }
        }

        public async Task CarregarHorariosAsync()
        {
            if (Marcacao == null)
                return;

            try
            {
                IsBusy = true;
                MensagemErro = string.Empty;

                var endpoint =
                    $"api/Marcacoes/disponibilidade" +
                    $"?funcionarioId={Marcacao.FuncionarioId}" +
                    $"&servicoId={Marcacao.ServicoId}" +
                    $"&data={DataSelecionada:yyyy-MM-dd}";

                var resultado = await _apiService.GetAsync<List<DateTime>>(endpoint);

                HorariosDisponiveis.Clear();

                if (resultado != null)
                {
                    foreach (var horario in resultado.OrderBy(h => h))
                        HorariosDisponiveis.Add(horario);
                }
            }
            catch
            {
                MensagemErro = "Não foi possível carregar os horários disponíveis.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public bool AReagendar
        {
            get => _aReagendar;
            private set
            {
                if (_aReagendar == value)
                    return;

                _aReagendar = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(PodeConfirmar));

                ConfirmarReagendamentoCommand.ChangeCanExecute();
            }
        }

        public bool ReagendamentoConcluido
        {
            get => _reagendamentoConcluido;
            private set
            {
                if (_reagendamentoConcluido == value)
                    return;

                _reagendamentoConcluido = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(PodeConfirmar));

                ConfirmarReagendamentoCommand.ChangeCanExecute();
            }
        }

        public bool PodeConfirmar =>
            Marcacao != null &&
            HorarioSelecionado.HasValue &&
            !AReagendar &&
            !ReagendamentoConcluido;

        public event PropertyChangedEventHandler? PropertyChanged;
        public Command ConfirmarReagendamentoCommand { get; }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task ConfirmarReagendamentoAsync()
        {
            if (!PodeConfirmar || Marcacao == null || !HorarioSelecionado.HasValue)
                return;

            try
            {
                AReagendar = true;
                MensagemErro = string.Empty;

                var request = new UpdateMarcacaoRequest
                {
                    ServicoId = Marcacao.ServicoId,
                    DataHoraInicio = HorarioSelecionado.Value,
                    Observacoes = Marcacao.Observacoes
                };

                var sucesso = await _apiService.PutAsync($"api/Marcacoes/{Marcacao.Id}", request);

                if (!sucesso)
                {
                    MensagemErro = "Não foi possível reagendar a marcação.";
                    return;
                }

                ReagendamentoConcluido = true;
            }
            catch
            {
                MensagemErro = "Não foi possível reagendar a marcação.";
            }
            finally
            {
                AReagendar = false;
            }
        }
    }
}