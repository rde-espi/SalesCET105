using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.Services.GoogleCalendarService
{
    public class GoogleCalendarSyncService : IGoogleCalendarSyncService
    {
        private readonly IGoogleCalendarContaRepository _contaRepository;
        private readonly IGoogleCalendarEventoRepository _eventoRepository;
        private readonly IGoogleCalendarService _googleCalendarService;

        public GoogleCalendarSyncService(IGoogleCalendarContaRepository contaRepository, IGoogleCalendarEventoRepository eventoRepository,
            IGoogleCalendarService googleCalendarService)
        {
            _contaRepository = contaRepository;
            _eventoRepository = eventoRepository;
            _googleCalendarService = googleCalendarService;
        }
        public async Task SincronizarCriacaoMarcacaoAsync(Marcacao marcacao, CancellationToken cancellationToken = default)
        {
            var titulo = $"Marcação - {marcacao.Servico.Nome}";

            var descricao =
                $"Serviço: {marcacao.Servico.Nome}\n" +
                $"Data: {marcacao.DataHoraInicio:dd/MM/yyyy}\n" +
                $"Hora: {marcacao.DataHoraInicio:HH:mm} - " +
                $"{marcacao.DataHoraFim:HH:mm}";

            // CLIENTE
            await CriarEventoParaUserAsync(
                marcacao,
                marcacao.ClienteId,
                titulo,
                descricao,
                cancellationToken);

            // FUNCIONÁRIO
            await CriarEventoParaUserAsync(
                marcacao,
                marcacao.Funcionario.UserId,
                titulo,
                descricao,
                cancellationToken);
        }

        public async Task SincronizarCancelamentoMarcacaoAsync(Marcacao marcacao,CancellationToken cancellationToken = default)
        {
            // CLIENTE
            await EliminarEventoParaUserAsync(
                marcacao,
                marcacao.ClienteId,
                cancellationToken);

            // FUNCIONÁRIO
            await EliminarEventoParaUserAsync(
                marcacao,
                marcacao.Funcionario.UserId,
                cancellationToken);
        }

        private async Task EliminarEventoParaUserAsync( Marcacao marcacao, string userId,CancellationToken cancellationToken)
        {
            var conta = await _contaRepository.GetByUserIdAsync(userId);

            if (conta == null || !conta.Ativo)
            {
                return;
            }

            var evento = await _eventoRepository.GetByMarcacaoAndUserAsync( marcacao.Id,userId);

            if (evento == null)
            {
                return;
            }

            await _googleCalendarService.EliminarEventoAsync(conta,evento.GoogleEventId, cancellationToken);

            await _eventoRepository.DeleteAsync(evento);
        }

        private async Task CriarEventoParaUserAsync(
            Marcacao marcacao,
            string userId,
            string titulo,
            string descricao,
            CancellationToken cancellationToken)
        {
            var conta = await _contaRepository.GetByUserIdAsync(userId);

            if (conta == null || !conta.Ativo)
            {
                return;
            }

            var existente = await _eventoRepository.GetByMarcacaoAndUserAsync(marcacao.Id, userId);

            if (existente != null)
            {
                return;
            }

            var googleEventId =
                await _googleCalendarService
                .CriarEventoAsync(
                    conta,
                    titulo,
                    descricao,
                    marcacao.DataHoraInicio,
                    marcacao.DataHoraFim,
                    cancellationToken);

            var evento = new GoogleCalendarEvento
            {
                MarcacaoId = marcacao.Id,
                UserId = userId,
                GoogleEventId = googleEventId,
                CalendarId = conta.CalendarId,
                DataCriacao = DateTime.UtcNow,
                DataUltimaSincronizacao = DateTime.UtcNow
            };

            await _eventoRepository.CreateAsync(evento);
        }

        public async Task SincronizarAtualizacaoMarcacaoAsync(Marcacao marcacao,CancellationToken cancellationToken = default)
        {
            var titulo = $"Marcação - {marcacao.Servico.Nome}";

            var descricao =
                $"Serviço: {marcacao.Servico.Nome}\n" +
                $"Data: {marcacao.DataHoraInicio:dd/MM/yyyy}\n" +
                $"Hora: {marcacao.DataHoraInicio:HH:mm} - " +
                $"{marcacao.DataHoraFim:HH:mm}";

            // CLIENTE
            await AtualizarEventoParaUserAsync(
                marcacao,
                marcacao.ClienteId,
                titulo,
                descricao,
                cancellationToken);

            // FUNCIONÁRIO
            await AtualizarEventoParaUserAsync(
                marcacao,
                marcacao.Funcionario.UserId,
                titulo,
                descricao,
                cancellationToken);
        }
        private async Task AtualizarEventoParaUserAsync(
            Marcacao marcacao,
            string userId,
            string titulo,
            string descricao,
            CancellationToken cancellationToken)
        {
            var conta = await _contaRepository.GetByUserIdAsync(userId);

            if (conta == null || !conta.Ativo)
            {
                return;
            }

            var evento = await _eventoRepository.GetByMarcacaoAndUserAsync(marcacao.Id, userId);
                        
            if (evento == null)
            {
                await CriarEventoParaUserAsync(
                    marcacao,
                    userId,
                    titulo,
                    descricao,
                    cancellationToken);

                return;
            }

            await _googleCalendarService.AtualizarEventoAsync(
                conta,
                evento.GoogleEventId,
                titulo,
                descricao,
                marcacao.DataHoraInicio,
                marcacao.DataHoraFim,
                cancellationToken);

            evento.DataUltimaSincronizacao = DateTime.UtcNow;

            await _eventoRepository.UpdateAsync(evento);
        }
    }
}
