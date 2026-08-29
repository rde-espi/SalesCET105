using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DispositivosController : ControllerBase
    {
        private readonly IDispositivoUserRepository _dispositivoUserRepository;

        public DispositivosController(IDispositivoUserRepository dispositivoUserRepository)
        {
            _dispositivoUserRepository = dispositivoUserRepository;
        }

        [HttpPost]
        public async Task<IActionResult> RegistarDispositivo(RegistarDispositivoDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var dispositivoExistente = await _dispositivoUserRepository.GetByFidAsync(dto.Fid);

            if (dispositivoExistente != null)
            {
                dispositivoExistente.UserId = userId;
                dispositivoExistente.Plataforma =
                    dto.Plataforma;
                dispositivoExistente.Ativo = true;
                dispositivoExistente.DataAtualizacao =
                    DateTime.Now;

                await _dispositivoUserRepository.UpdateAsync(dispositivoExistente);

                return NoContent();
            }

            var dispositivo = new DispositivoUser
            {
                UserId = userId,
                Fid = dto.Fid,
                Plataforma = dto.Plataforma,
                Ativo = true,
                DataCriacao = DateTime.Now
            };

            await _dispositivoUserRepository
                .CreateAsync(dispositivo);

            return Ok(new
            {
                dispositivo.Id
            });
        }

    }
}
