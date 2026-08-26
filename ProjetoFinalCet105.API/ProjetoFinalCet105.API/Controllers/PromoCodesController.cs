using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.PromoCodes;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromoCodesController : ControllerBase
    {
        private readonly IPromoCodeRepository _promoCodeRepository;
        private readonly ValidarPromoCodeUseCase _validarPromoCodeUseCase;

        public PromoCodesController(
            IPromoCodeRepository promoCodeRepository,
            ValidarPromoCodeUseCase validarPromoCodeUseCase)
        {
            _promoCodeRepository = promoCodeRepository;
            _validarPromoCodeUseCase = validarPromoCodeUseCase;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromoCodeDTO>>> GetAll()
        {
            var promoCodes = await _promoCodeRepository
                .GetAll()
                .OrderByDescending(p => p.DataInicio)
                .Select(p => new PromoCodeDTO
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Descricao = p.Descricao,
                    PercentagemDesconto = p.PercentagemDesconto,
                    DataInicio = p.DataInicio,
                    DataFim = p.DataFim,
                    LimiteUtilizacoes = p.LimiteUtilizacoes,
                    NumeroUtilizacoes = p.NumeroUtilizacoes,
                    Ativo = p.Ativo
                })
                .ToListAsync();

            return Ok(promoCodes);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PromoCodeDTO>> GetById(int id)
        {
            var promoCode =
                await _promoCodeRepository.GetByIdAsync(id);

            if (promoCode == null)
            {
                return NotFound("Código promocional não encontrado.");
            }

            var dto = new PromoCodeDTO
            {
                Id = promoCode.Id,
                Codigo = promoCode.Codigo,
                Descricao = promoCode.Descricao,
                PercentagemDesconto = promoCode.PercentagemDesconto,
                DataInicio = promoCode.DataInicio,
                DataFim = promoCode.DataFim,
                LimiteUtilizacoes = promoCode.LimiteUtilizacoes,
                NumeroUtilizacoes = promoCode.NumeroUtilizacoes,
                Ativo = promoCode.Ativo
            };

            return Ok(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<PromoCodeDTO>> Create(CriarPromoCodeDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Codigo))
            {
                return BadRequest("O código é obrigatório.");
            }

            if (dto.PercentagemDesconto <= 0 ||
                dto.PercentagemDesconto > 100)
            {
                return BadRequest(
                    "A percentagem de desconto deve estar entre 0 e 100.");
            }

            if (dto.DataFim <= dto.DataInicio)
            {
                return BadRequest(
                    "A data final deve ser posterior à data inicial.");
            }

            if (dto.LimiteUtilizacoes.HasValue &&
                dto.LimiteUtilizacoes.Value <= 0)
            {
                return BadRequest(
                    "O limite de utilizações deve ser superior a zero.");
            }

            var codigo = dto.Codigo.Trim().ToUpper();

            var existente = await _promoCodeRepository.GetByCodigoAsync(codigo);

            if (existente != null)
            {
                return Conflict("Já existe um código promocional com este código.");
            }

            var promoCode = new PromoCode
            {
                Codigo = codigo,
                Descricao = dto.Descricao?.Trim(),
                PercentagemDesconto = dto.PercentagemDesconto,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                LimiteUtilizacoes = dto.LimiteUtilizacoes,
                NumeroUtilizacoes = 0,
                Ativo = true
            };

            await _promoCodeRepository.CreateAsync(promoCode);

            var resposta = new PromoCodeDTO
            {
                Id = promoCode.Id,
                Codigo = promoCode.Codigo,
                Descricao = promoCode.Descricao,
                PercentagemDesconto = promoCode.PercentagemDesconto,
                DataInicio = promoCode.DataInicio,
                DataFim = promoCode.DataFim,
                LimiteUtilizacoes = promoCode.LimiteUtilizacoes,
                NumeroUtilizacoes = promoCode.NumeroUtilizacoes,
                Ativo = promoCode.Ativo
            };

            return CreatedAtAction(
                nameof(GetAll),
                resposta);
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost("validar")]
        public async Task<ActionResult<PromoCodeValidadoDTO>> Validar(ValidarPromoCodeDTO dto)
        {
            var clienteId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (clienteId == null)
            {
                return Unauthorized();
            }

            var resultado = await _validarPromoCodeUseCase.ExecuteAsync(dto.Codigo, clienteId);

            if (!resultado.Sucesso)
            {
                return BadRequest(resultado.Erro);
            }

            return Ok(resultado.Dados);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id,UpdatePromoCodeDTO dto)
        {
            var promoCode = await _promoCodeRepository.GetByIdAsync(id);

            if (promoCode == null)
            {
                return NotFound("Código promocional não encontrado.");
            }

            if (string.IsNullOrWhiteSpace(dto.Codigo))
            {
                return BadRequest("O código é obrigatório.");
            }

            if (dto.PercentagemDesconto <= 0 ||
                dto.PercentagemDesconto > 100)
            {
                return BadRequest(
                    "A percentagem de desconto deve estar entre 0 e 100.");
            }

            if (dto.DataFim <= dto.DataInicio)
            {
                return BadRequest(
                    "A data final deve ser posterior à data inicial.");
            }

            if (dto.LimiteUtilizacoes.HasValue &&
                dto.LimiteUtilizacoes.Value <= 0)
            {
                return BadRequest(
                    "O limite de utilizações deve ser superior a zero.");
            }

            var codigo = dto.Codigo.Trim().ToUpper();

            var existente = await _promoCodeRepository.GetByCodigoAsync(codigo);

            if (existente != null && existente.Id != promoCode.Id)
            {
                return Conflict(
                    "Já existe outro código promocional com este código.");
            }

            promoCode.Codigo = codigo;
            promoCode.Descricao = dto.Descricao?.Trim();
            promoCode.PercentagemDesconto =dto.PercentagemDesconto;
            promoCode.DataInicio = dto.DataInicio;
            promoCode.DataFim = dto.DataFim;
            promoCode.LimiteUtilizacoes = dto.LimiteUtilizacoes;

            await _promoCodeRepository.UpdateAsync(promoCode);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int}/ativo")]
        public async Task<IActionResult> UpdateAtivo(int id,UpdatePromoCodeAtivoDTO dto)
        {
            var promoCode =
                await _promoCodeRepository.GetByIdAsync(id);

            if (promoCode == null)
            {
                return NotFound("Código promocional não encontrado.");
            }

            promoCode.Ativo = dto.Ativo;

            await _promoCodeRepository.UpdateAsync(promoCode);

            return NoContent();
        }
    }
}