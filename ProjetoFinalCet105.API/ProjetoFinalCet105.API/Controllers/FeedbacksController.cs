using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.UseCases.Feedbacks;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : BaseApiController
    {
        private readonly CreateFeedbackUseCase _createFeedbackUseCase;
        private readonly GetFeedbackByIdUseCase _getFeedbackByIdUseCase;
        private readonly GetFeedbacksByFuncionarioUseCase _getFeedbacksByFuncionarioUseCase;
        private readonly GetFeedbackResumoFuncionarioUseCase _getFeedbackResumoFuncionarioUseCase;
        private readonly UpdateFeedbackUseCase _updateFeedbackUseCase;
        private readonly DeleteFeedbackUseCase _deleteFeedbackUseCase;
        private readonly GetAllFeedbacksUseCase _getAllFeedbacksUseCase;
        private readonly GetMeusFeedbacksUseCase _getMeusFeedbacksUseCase;
        private readonly GetMeuFeedbackResumoUseCase _getMeuFeedbackResumoUseCase;
        private readonly GetFeedbackByMarcacaoUseCase _getFeedbackByMarcacaoUseCase;

        public FeedbacksController(
            CreateFeedbackUseCase createFeedbackUseCase,
            GetFeedbackByIdUseCase getFeedbackByIdUseCase,
            GetFeedbacksByFuncionarioUseCase getFeedbacksByFuncionarioUseCase,
            GetFeedbackResumoFuncionarioUseCase getFeedbackResumoFuncionarioUseCase,
            UpdateFeedbackUseCase updateFeedbackUseCase,
            DeleteFeedbackUseCase deleteFeedbackUseCase,
            GetAllFeedbacksUseCase getAllFeedbacksUseCase,
            GetMeusFeedbacksUseCase getMeusFeedbacksUseCase,
            GetMeuFeedbackResumoUseCase getMeuFeedbackResumoUseCase,
                GetFeedbackByMarcacaoUseCase getFeedbackByMarcacaoUseCase)
        {
            _createFeedbackUseCase = createFeedbackUseCase;
            _getFeedbackByIdUseCase = getFeedbackByIdUseCase;
            _getFeedbacksByFuncionarioUseCase = getFeedbacksByFuncionarioUseCase;
            _getFeedbackResumoFuncionarioUseCase = getFeedbackResumoFuncionarioUseCase;
            _updateFeedbackUseCase = updateFeedbackUseCase;
            _deleteFeedbackUseCase = deleteFeedbackUseCase;
            _getAllFeedbacksUseCase = getAllFeedbacksUseCase;
            _getMeusFeedbacksUseCase = getMeusFeedbacksUseCase;
            _getMeuFeedbackResumoUseCase = getMeuFeedbackResumoUseCase;
            _getFeedbackByMarcacaoUseCase = getFeedbackByMarcacaoUseCase;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FeedbackDTO>>> GetAllFeedbacks()
        {
            var resultado = await _getAllFeedbacksUseCase.ExecuteAsync();

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }


        [Authorize(Roles = "Funcionario")]
        [HttpGet("meus")]
        public async Task<ActionResult<IEnumerable<FeedbackDTO>>> GetMeusFeedbacks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var resultado = await _getMeusFeedbacksUseCase.ExecuteAsync(userId);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }

        [Authorize(Roles = "Funcionario")]
        [HttpGet("meus/resumo")]
        public async Task<ActionResult<FeedbackResumoDTO>> GetMeuFeedbackResumo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var resultado = await _getMeuFeedbackResumoUseCase.ExecuteAsync(userId);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }

        [HttpGet("funcionario/{funcionarioId:int}")]
        public async Task<ActionResult<IEnumerable<FeedbackDTO>>> GetFeedbacksByFuncionario(int funcionarioId)
        {
            var resultado = await _getFeedbacksByFuncionarioUseCase.ExecuteAsync(funcionarioId);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<FeedbackDTO>> GetFeedbackById(int id)
        {
            var resultado = await _getFeedbackByIdUseCase.ExecuteAsync(id);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost]
        public async Task<ActionResult<FeedbackDTO>> CreateFeedback(NovoFeedbackDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado = await _createFeedbackUseCase.ExecuteAsync(userId, dto);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return CreatedAtAction(nameof(GetFeedbackById), new { id = resultado.Dados!.Id }, resultado.Dados);
        }

        [HttpGet("funcionario/{funcionarioId:int}/resumo")]
        public async Task<ActionResult<FeedbackResumoDTO>> GetFeedbackResumoFuncionario(int funcionarioId)
        {
            var resultado = await _getFeedbackResumoFuncionarioUseCase.ExecuteAsync(funcionarioId);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }

        [Authorize(Policy = "FeedbackMarcação")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFeedback(int id, UpdateFeedbackDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado = await _updateFeedbackUseCase.ExecuteAsync( id, userId, User.IsInRole("Admin"), dto);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return NoContent();
        }

        [Authorize(Policy = "FeedbackMarcação")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado = await _deleteFeedbackUseCase.ExecuteAsync( id, userId, User.IsInRole("Admin"));

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return NoContent();
        }

        [Authorize(Roles = "Cliente,Admin")]
        [HttpGet("marcacao/{marcacaoId:int}")]
        public async Task<ActionResult<FeedbackDTO>> GetFeedbackByMarcacao( int marcacaoId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var resultado = await _getFeedbackByMarcacaoUseCase.ExecuteAsync( marcacaoId, userId, User.IsInRole("Admin"));

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }
    }
}