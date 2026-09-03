using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.UseCases.Cliente;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : BaseApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly CreateClienteUseCase _createClienteUseCase;
        private readonly UpdateClienteUseCase _updateClienteUseCase;

        public ClientesController(UserManager<User> userManager, CreateClienteUseCase createClienteUseCase,UpdateClienteUseCase updateClienteUseCase)
        {
            _userManager = userManager;
            _createClienteUseCase = createClienteUseCase;
            _updateClienteUseCase = updateClienteUseCase;
        }

        [HttpPost]
        public async Task<ActionResult<ClienteDTO>> CreateCliente(NovoClienteDTO dto)
        {
            var resultado = await _createClienteUseCase.ExecuteAsync(dto);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return CreatedAtAction(nameof(GetClienteById), new { id = resultado.Dados!.Id }, resultado.Dados);
        }

        [Authorize(Policy = "ConsultarCliente")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDTO>> GetClienteById(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            // Cliente só pode consultar o próprio perfil
            if (User.IsInRole("Cliente") &&
                !User.IsInRole("Admin") &&
                userId != id)
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (!await _userManager.IsInRoleAsync(user, "Cliente"))
            {
                return NotFound();
            }

            return Ok(new ClienteDTO
            {
                Id = user.Id,
                NomeCompleto = user.NomeCompleto,
                Email = user.Email!,
                Telefone = user.PhoneNumber,
                Contribuinte = user.Contribuinte,
                Morada = user.Morada,
                CodigoPostal = user.CodigoPostal,
                Localidade = user.Localidade,
                FotografiaUrl = user.FotografiaUrl,
                Ativo = user.Ativo,
                DataCriacao = user.DataCriacao,
                DataAtualizacao = user.DataAtualizacao
            });
        }

        [Authorize(Policy = "AlterarCliente")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCliente(string id,UpdateClienteDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado = await _updateClienteUseCase.ExecuteAsync( id, userId, User.IsInRole("Cliente"), User.IsInRole("Admin"), dto);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return NoContent();
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDTO>>> GetAllClientes()
        {
            var clientes = await _userManager.GetUsersInRoleAsync("Cliente");

            var resultado = clientes
                .Select(user => new ClienteDTO
                {
                    Id = user.Id,
                    NomeCompleto = user.NomeCompleto,
                    Email = user.Email!,
                    Telefone = user.PhoneNumber,
                    Contribuinte = user.Contribuinte,
                    Morada = user.Morada,
                    CodigoPostal = user.CodigoPostal,
                    Localidade = user.Localidade,
                    FotografiaUrl = user.FotografiaUrl,
                    Ativo = user.Ativo,
                    DataCriacao = user.DataCriacao,
                    DataAtualizacao = user.DataAtualizacao
                })
                .OrderBy(c => c.NomeCompleto)
                .ToList();

            return Ok(resultado);
        }
    }
}
