using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.UseCases.Dashboard
{
    public class GetDashboardClientesUseCase
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMarcacaoRepository _marcacaoRepository;

        public GetDashboardClientesUseCase(IClienteRepository clienteRepository, IMarcacaoRepository marcacaoRepository)
        {
            _clienteRepository = clienteRepository;
            _marcacaoRepository = marcacaoRepository;
        }

        public async Task<DashboardClientesDTO> ExecuteAsync()
        {
            var hoje = DateTime.Today;

            var inicioMes = new DateTime(hoje.Year,  hoje.Month, 1);

            var clientes = await _clienteRepository.GetAllClientesAsync();

            var totalClientes = clientes.Count;

            var novosClientesMes = clientes.Count(c =>
                c.DataCriacao >= inicioMes &&
                c.DataCriacao < inicioMes.AddMonths(1));

            var marcacoesValidas = _marcacaoRepository
                .GetAllWithDetails()
                .Where(m =>
                    m.EstadoMarcacao.Nome != "Cancelada");

            var clientesRecorrentes = await marcacoesValidas
                .GroupBy(m => m.ClienteId)
                .Where(g => g.Count() >= 2)
                .CountAsync();

            var limite60Dias = hoje.AddDays(-60);
            var limite90Dias = hoje.AddDays(-90);

            var ultimasMarcacoesClientes = await marcacoesValidas
                .GroupBy(m => m.ClienteId)
                .Select(g => new
                {
                    ClienteId = g.Key,
                    UltimaMarcacao = g.Max(m => m.DataHoraInicio)
                })
                .ToListAsync();

            var clientesInativos60Dias = ultimasMarcacoesClientes.Count(c => c.UltimaMarcacao < limite60Dias);

            var clientesInativos90Dias = ultimasMarcacoesClientes.Count(c =>c.UltimaMarcacao < limite90Dias);

            var taxaRecorrencia =
                totalClientes > 0
                    ? Math.Round(
                        (decimal)clientesRecorrentes /
                        totalClientes * 100,
                        2)
                    : 0;

            return new DashboardClientesDTO
            {
                TotalClientes = totalClientes,
                NovosClientesMes = novosClientesMes,
                ClientesRecorrentes = clientesRecorrentes,

                ClientesInativos60Dias = clientesInativos60Dias,
                ClientesInativos90Dias = clientesInativos90Dias,

                TaxaRecorrencia = taxaRecorrencia
            };
        }
    }
}
