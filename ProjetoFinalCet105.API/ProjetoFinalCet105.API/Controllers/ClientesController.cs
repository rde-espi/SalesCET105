using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public ClientesController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<ActionResult<ClienteDTO>> CreateCliente(NovoClienteDTO dto)
        {
            var userExistente = await _userManager.FindByEmailAsync(dto.Email);

            if (userExistente != null)
            {
                return BadRequest("Já existe um utilizador com este email");
            }

            var user = new User
            {
                NomeCompleto = dto.NomeCompleto,
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.Telefone,
                FotografiaUrl = dto.FotografiaUrl,
                Ativo = true,
                DataCriacao = DateTime.Now
            };

            var resultado = await _userManager.CreateAsync(user, dto.Password);

            if (!resultado.Succeeded)
            {
                return BadRequest(resultado.Errors);
            }

            var resultadoRole = await _userManager.AddToRoleAsync(user, "Cliente");

            if (!resultadoRole.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(resultadoRole.Errors); 
            }

            var clienteDTO = new ClienteDTO
            {
                Id = user.Id,
                NomeCompleto = user.NomeCompleto,
                Email = user.Email,
                Telefone = user.PhoneNumber,
                FotografiaUrl = user.FotografiaUrl,
                Ativo = user.Ativo,
                DataCriacao = user.DataCriacao,
                DataAtualizacao = user.DataAtualizacao
            };

            return CreatedAtAction(nameof(GetClienteById), new { id = user.Id }, clienteDTO);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDTO>> GetClienteById(string id)
        {
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
                FotografiaUrl = user.FotografiaUrl,
                Ativo = user.Ativo,
                DataCriacao = user.DataCriacao,
                DataAtualizacao = user.DataAtualizacao
            });
        }

    }
}
