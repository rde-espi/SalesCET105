using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuncionariosController : ControllerBase
    {
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly UserManager<User> _userManager;

        public FuncionariosController(IFuncionarioRepository funcionarioRepository,UserManager<User> userManager)
        {
            _funcionarioRepository = funcionarioRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FuncionarioDTO>>> GetAllFuncionarios()
        {
            var funcionarios = await _funcionarioRepository.GetAllFuncionariosWithUser()
                .Select(f => new FuncionarioDTO
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    NomeCompleto = f.User.NomeCompleto,
                    Email = f.User.Email,
                    Telefone = f.User.PhoneNumber,
                    FotografiaUrl = f.User.FotografiaUrl,
                    Biografia = f.Biografia,
                    DataAdmissao = f.DataAdmissao,
                    Disponivel = f.Disponivel,
                    Ativo = f.Ativo
                })
                .ToListAsync();
            return Ok(funcionarios);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<FuncionarioDTO>> GetFuncionarioById(int id)
        {
            var funcionario = await _funcionarioRepository.GetFuncionarioByIdAsync(id);
            if(funcionario == null)
            {
                return NotFound();
            }
            return Ok(new FuncionarioDTO
            {
                Id = funcionario.Id,
                UserId = funcionario.UserId,
                NomeCompleto = funcionario.User.NomeCompleto,
                Email = funcionario.User.Email,
                Telefone = funcionario.User.PhoneNumber,
                FotografiaUrl = funcionario.User.FotografiaUrl,
                Biografia = funcionario.Biografia,
                DataAdmissao = funcionario.DataAdmissao,
                Disponivel = funcionario.Disponivel,
                Ativo = funcionario.Ativo
            });
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<FuncionarioDTO>> GetFuncionarioByUserId(string userId)
        {
            var funcionario = await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);
            if (funcionario == null)
            {
                return NotFound();
            }
            return Ok(new FuncionarioDTO
            {
                Id = funcionario.Id,
                UserId = funcionario.UserId,
                NomeCompleto = funcionario.User.NomeCompleto,
                Email = funcionario.User.Email,
                Telefone = funcionario.User.PhoneNumber,
                FotografiaUrl = funcionario.User.FotografiaUrl,
                Biografia = funcionario.Biografia,
                DataAdmissao = funcionario.DataAdmissao,
                Disponivel = funcionario.Disponivel,
                Ativo = funcionario.Ativo
            });
        }

        [HttpPost]
        public async Task<ActionResult<FuncionarioDTO>>CreateFuncionario(NovoFuncionarioDTO dto)
        {
            var userExistente = await _userManager.FindByEmailAsync(dto.Email);
            if(userExistente != null)
            {
                return BadRequest("Ja existe um utilizador com este email");
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

            var resultadoUser = await _userManager.CreateAsync(user,dto.Password);

            if (!resultadoUser.Succeeded)
            {
                return BadRequest(resultadoUser.Errors);
            }

            var resultadoRole = await _userManager.AddToRoleAsync(user, "Funcionario");
            if (!resultadoRole.Succeeded)
            {
                return BadRequest(resultadoRole.Errors);
            }

            var funcionario = new Funcionario
            {
                UserId = user.Id,
                Biografia = dto.Biografia,
                DataAdmissao = dto.DataAdmissao,
                Disponivel = dto.Disponivel,
                Ativo = true
            };

            try
            {
                await _funcionarioRepository.CreateAsync(funcionario);
            }
            catch (Exception)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest();
            }

            var funcionarioDto = new FuncionarioDTO
            {
                Id = funcionario.Id,
                UserId = user.Id,
                NomeCompleto = user.NomeCompleto,
                Email = user.Email,
                Telefone = user.PhoneNumber,
                FotografiaUrl = user.FotografiaUrl,
                Biografia = funcionario.Biografia,
                DataAdmissao = funcionario.DataAdmissao,
                Disponivel = funcionario.Disponivel,
                Ativo = funcionario.Ativo
            };

            return CreatedAtAction(
                nameof(GetFuncionarioById),
                new { id = funcionario.Id },
                funcionarioDto);
        }
    }
}
