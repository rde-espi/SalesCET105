using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Services.NifService;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class NifController : ControllerBase
    {
        private readonly INifService _nifService;

        public NifController(INifService nifService)
        {
            _nifService = nifService;
        }

        [HttpPost("validar")]
        [EnableRateLimiting("NifValidation")]
        public async Task<ActionResult<ResultadoValidacaoNifDTO>> Validar([FromBody] ValidarNifDTO dto)
        {
            var resultado = await _nifService.ValidarAsync(dto.Nif);

            return Ok(resultado);
        }
    }
}
