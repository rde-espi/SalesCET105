using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinalCet105.API.UseCases.Dashboard;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly GetDashboardFinanceiroUseCase
            _getDashboardFinanceiroUseCase;
        private readonly GetDashboardAgendaUseCase _getDashboardAgendaUseCase;
        private readonly GetDashboardClientesUseCase _getDashboardClientesUseCase;
        private readonly GetDashboardEquipaUseCase _getDashboardEquipaUseCase;

        public DashboardController( GetDashboardFinanceiroUseCase getDashboardFinanceiroUseCase, GetDashboardAgendaUseCase getDashboardAgendaUseCase, 
            GetDashboardClientesUseCase getDashboardClientesUseCase, GetDashboardEquipaUseCase getDashboardEquipaUseCase)
        {
            _getDashboardFinanceiroUseCase = getDashboardFinanceiroUseCase;
            _getDashboardAgendaUseCase = getDashboardAgendaUseCase;
            _getDashboardClientesUseCase = getDashboardClientesUseCase;
            _getDashboardEquipaUseCase = getDashboardEquipaUseCase;
        }

        [HttpGet("financeiro")]
        public async Task<IActionResult> GetFinanceiro()
        {
            var resultado = await _getDashboardFinanceiroUseCase.ExecuteAsync();

            return Ok(resultado);
        }

        [HttpGet("financeiro/evolucao-mensal")]
        public async Task<IActionResult> GetEvolucaoMensal([FromQuery] int? ano = null)
        {
            var resultado = await _getDashboardFinanceiroUseCase.ExecuteEvolucaoMensalAsync(ano);

            return Ok(resultado);
        }

        [HttpGet("financeiro/servicos")]
        public async Task<IActionResult> GetServicosMaisFaturados([FromQuery] int limite = 5)
        {
            if (limite <= 0)
                limite = 5;

            if (limite > 50)
                limite = 50;

            var resultado = await _getDashboardFinanceiroUseCase.ExecuteServicosMaisFaturadosAsync(limite);

            return Ok(resultado);
        }

        [HttpGet("financeiro/categorias")]
        public async Task<IActionResult> GetFaturacaoPorCategoria()
        {
            var resultado = await _getDashboardFinanceiroUseCase.ExecuteFaturacaoPorCategoriaAsync();

            return Ok(resultado);
        }


        [HttpGet("agenda")]
        public async Task<IActionResult> GetAgenda()
        {
            var resultado = await _getDashboardAgendaUseCase.ExecuteAsync();

            return Ok(resultado);
        }

        [HttpGet("agenda/horarios-procura")]
        public async Task<IActionResult> GetHorariosMaiorProcura([FromQuery] int limite = 5)
        {
            if (limite <= 0)
                limite = 5;

            if (limite > 24)
                limite = 24;

            var resultado = await _getDashboardAgendaUseCase.ExecuteHorariosMaiorProcuraAsync(limite);

            return Ok(resultado);
        }

        [HttpGet("agenda/servicos")]
        public async Task<IActionResult> GetServicosMaisMarcados([FromQuery] int limite = 5)
        {
            if (limite <= 0)
                limite = 5;

            if (limite > 50)
                limite = 50;

            var resultado = await _getDashboardAgendaUseCase.ExecuteServicosMaisMarcadosAsync(limite);

            return Ok(resultado);
        }

        [HttpGet("agenda/dias-procura")]
        public async Task<IActionResult> GetDiasMaiorProcura()
        {
            var resultado = await _getDashboardAgendaUseCase.ExecuteDiasMaiorProcuraAsync();

            return Ok(resultado);
        }

        [HttpGet("clientes")]
        public async Task<IActionResult> GetClientes()
        {
            var resultado = await _getDashboardClientesUseCase.ExecuteAsync();

            return Ok(resultado);
        }

        [HttpGet("equipa")]
        public async Task<IActionResult> GetEquipa()
        {
            var resultado =
                await _getDashboardEquipaUseCase.ExecuteAsync();

            return Ok(resultado);
        }
    }
}
