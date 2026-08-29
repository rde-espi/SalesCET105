using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.AuthService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Funcionarios
{
    public class CreateFuncionarioUseCase
    {
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly UserManager<User> _userManager;
        private readonly IAuthService _authService;
        private readonly ILogger<CreateFuncionarioUseCase> _logger;

        public CreateFuncionarioUseCase(
            IFuncionarioRepository funcionarioRepository,
            UserManager<User> userManager, IAuthService authService, ILogger<CreateFuncionarioUseCase> logger)
        {
            _funcionarioRepository = funcionarioRepository;
            _userManager = userManager;
            _authService = authService;
            _logger = logger;
        }

        public async Task<UseCaseResult<FuncionarioDTO>> ExecuteAsync(
            NovoFuncionarioDTO dto)
        {
            var userExistente =
                await _userManager.FindByEmailAsync(dto.Email);

            if (userExistente != null)
            {
                return UseCaseResult<FuncionarioDTO>.Falha(
                    "Já existe um utilizador com este email.",
                    TipoErro.Conflito);
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

            var resultadoUser =
                await _userManager.CreateAsync(user, dto.Password);

            if (!resultadoUser.Succeeded)
            {
                var erros = string.Join(
                    "; ",
                    resultadoUser.Errors.Select(e => e.Description));

                return UseCaseResult<FuncionarioDTO>.Falha(erros);
            }

            var resultadoRole =
                await _userManager.AddToRoleAsync(
                    user,
                    "Funcionario");

            if (!resultadoRole.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                var erros = string.Join(
                    "; ",
                    resultadoRole.Errors.Select(e => e.Description));

                return UseCaseResult<FuncionarioDTO>.Falha(erros);
            }

            Funcionario funcionario;

            try
            {
                funcionario = new Funcionario
                {
                    UserId = user.Id,
                    Biografia = dto.Biografia,
                    DataAdmissao = dto.DataAdmissao,
                    Disponivel = dto.Disponivel,
                    Ativo = true
                };

                await _funcionarioRepository.CreateAsync(funcionario);
            }
            catch (Exception)
            {
                await _userManager.DeleteAsync(user);

                return UseCaseResult<FuncionarioDTO>.Falha(
                    "Ocorreu um erro ao criar o funcionário.");
            }

            var resposta = new FuncionarioDTO
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

            
            try
            {
                await _authService
                    .EnviarConfirmacaoEmailAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,"O funcionário {FuncionarioId} foi criado, mas ocorreu uma falha ao enviar o email de confirmação.",funcionario.Id);
            }

            return UseCaseResult<FuncionarioDTO>.Ok(resposta);
        }
    }
}
