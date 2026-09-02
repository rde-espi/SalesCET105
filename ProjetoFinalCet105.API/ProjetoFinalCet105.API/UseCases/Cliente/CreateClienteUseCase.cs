using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Services.AuthService;
using ProjetoFinalCet105.API.Services.NifService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Cliente
{
    public class CreateClienteUseCase
    {
        private readonly UserManager<User> _userManager;
        private readonly IAuthService _authService;
        private readonly ILogger<CreateClienteUseCase> _logger;
        private readonly INifService _nifService;

        public CreateClienteUseCase(UserManager<User> userManager, IAuthService authService, ILogger<CreateClienteUseCase> logger,INifService nifService)
        {
            _userManager = userManager;
            _authService = authService;
            _logger = logger;
            _nifService = nifService;
        }

        public async Task<UseCaseResult<ClienteDTO>> ExecuteAsync( NovoClienteDTO dto)
        {
            var userExistente = await _userManager.FindByEmailAsync(dto.Email);

            if (userExistente != null)
            {
                return UseCaseResult<ClienteDTO>.Falha("Já existe um utilizador com este email.", TipoErro.Conflito);
            }

            if (!string.IsNullOrWhiteSpace(dto.Contribuinte))
            {
                dto.Contribuinte = dto.Contribuinte.Trim();

                if (!_nifService.ValidarNifPortugues(dto.Contribuinte))
                {
                    return UseCaseResult<ClienteDTO>.Falha("O NIF indicado não é válido.");
                }
            }


            var user = new User
            {
                NomeCompleto = dto.NomeCompleto,
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.Telefone,
                Contribuinte = dto.Contribuinte,
                Morada = dto.Morada,
                CodigoPostal = dto.CodigoPostal,
                Localidade = dto.Localidade,
                FotografiaUrl = dto.FotografiaUrl,
                Ativo = true,
                DataCriacao = DateTime.Now
            };

            

            var resultado = await _userManager.CreateAsync( user, dto.Password);

            if (!resultado.Succeeded)
            {
                var erros = string.Join( "; ", resultado.Errors.Select(e => e.Description));

                return UseCaseResult<ClienteDTO>.Falha(erros);
            }

            var resultadoRole = await _userManager.AddToRoleAsync( user,"Cliente");

            if (!resultadoRole.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                var erros = string.Join("; ",resultadoRole.Errors.Select(e => e.Description));

                return UseCaseResult<ClienteDTO>.Falha(erros);
            }

            var resposta = new ClienteDTO
            {
                Id = user.Id,
                NomeCompleto = user.NomeCompleto,
                Email = user.Email,
                Telefone = user.PhoneNumber,
                Contribuinte = user.Contribuinte,
                Morada = user.Morada,
                CodigoPostal = user.CodigoPostal,
                Localidade = user.Localidade,
                FotografiaUrl = user.FotografiaUrl,
                Ativo = user.Ativo,
                DataCriacao = user.DataCriacao,
                DataAtualizacao = user.DataAtualizacao
            };
            try
            {
                await _authService.EnviarConfirmacaoEmailAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,"O cliente {ClienteId} foi criado, mas ocorreu uma falha ao enviar o email de confirmação.",user.Id);
            }

            return UseCaseResult<ClienteDTO>.Ok(resposta);
        }
    }
}
