using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.UseCases.Admin;
using ProjetoFinalCet105.API.UseCases.Common;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AlterarRoleUserUseCase _alterarRoleUserUseCase;
        private readonly ConcederPermissaoAdminTemporariaUseCase _concederPermissaoAdminTemporariaUseCase;
        private readonly RevogarPermissaoAdminTemporariaUseCase _revogarPermissaoAdminTemporariaUseCase;

        public AdminController(AlterarRoleUserUseCase alterarRoleUserUseCase,
            ConcederPermissaoAdminTemporariaUseCase concederPermissaoAdminTemporariaUseCase,
            RevogarPermissaoAdminTemporariaUseCase revogarPermissaoAdminTemporariaUseCase)
        {
            _alterarRoleUserUseCase = alterarRoleUserUseCase;
            _concederPermissaoAdminTemporariaUseCase = concederPermissaoAdminTemporariaUseCase;
            _revogarPermissaoAdminTemporariaUseCase = revogarPermissaoAdminTemporariaUseCase;
        }

        [HttpPatch("users/{userId}/role")]
        public async Task<IActionResult> AlterarRole(string userId, [FromBody] AlterarRoleUserDTO dto)
        {
            var adminAtualId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(adminAtualId))
                return Unauthorized();

            var resultado = await _alterarRoleUserUseCase.ExecuteAsync( userId, adminAtualId,dto);

            if (!resultado.Sucesso)
            {
                return resultado.TipoErro switch
                {
                    TipoErro.NaoEncontrado => NotFound(resultado.Erro),

                    _ => BadRequest(resultado.Erro)
                };
            }

            return Ok(new
            {
                mensagem = $"Role alterada com sucesso para {dto.NovaRole}."
            });
        }

        [HttpPost("permissoes-temporarias")]
        public async Task<IActionResult> ConcederPermissaoTemporaria([FromBody] ConcederPermissaoAdminTemporariaDTO dto)
        {
            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(adminUserId))
                return Unauthorized();

            var resultado = await _concederPermissaoAdminTemporariaUseCase.ExecuteAsync( adminUserId, dto);

            if (!resultado.Sucesso)
            {
                return resultado.TipoErro switch
                {
                    TipoErro.NaoEncontrado => NotFound(resultado.Erro),
                    _ => BadRequest(resultado.Erro)
                };
            }

            return Ok(new
            {
                mensagem = $"Privilégios administrativos temporários concedidos durante {dto.DuracaoMinutos} minutos.",
                permissaoId = resultado.Dados
            });
        }

        [HttpPatch("permissoes-temporarias/{id}/revogar")]
        public async Task<IActionResult> RevogarPermissaoTemporaria(int id)
        {
            var resultado = await _revogarPermissaoAdminTemporariaUseCase.ExecuteAsync(id);

            if (!resultado.Sucesso)
            {
                return resultado.TipoErro switch
                {
                    TipoErro.NaoEncontrado => NotFound(resultado.Erro),
                    _ => BadRequest(resultado.Erro)
                };
            }

            return Ok(new
            {
                mensagem = "Permissão administrativa temporária revogada com sucesso."
            });
        }
    }
}
