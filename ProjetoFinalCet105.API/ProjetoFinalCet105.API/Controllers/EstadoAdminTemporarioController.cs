using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.API.Services.AdminTemporario;

namespace ProjetoFinalCet105.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EstadoAdminTemporarioController : ControllerBase
{
    private readonly AdminTemporarioService _adminTemporarioService;

    public EstadoAdminTemporarioController(AdminTemporarioService adminTemporarioService)
    {
        _adminTemporarioService = adminTemporarioService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMeuEstado()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (!User.IsInRole("Funcionario"))
        {
            return Ok(new
            {
                ativo = false,
                dataInicio = (DateTime?)null,
                dataFim = (DateTime?)null
            });
        }

        var estado = await _adminTemporarioService.ObterEstadoAsync(userId);

        return Ok(new
        {
            ativo = estado.Ativo,
            dataInicio = estado.DataInicio,
            dataFim = estado.DataFim
        });
    }
}