using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Repositories;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Authorization
{
    public class AdminOuAdminTemporarioHandler : AuthorizationHandler<AdminOuAdminTemporarioRequirement>
    {
        private readonly IPermissaoAdminTemporariaRepository _permissaoRepository;

        public AdminOuAdminTemporarioHandler(IPermissaoAdminTemporariaRepository permissaoRepository)
        {
            _permissaoRepository = permissaoRepository;
        }

        protected override async Task HandleRequirementAsync( AuthorizationHandlerContext context, AdminOuAdminTemporarioRequirement requirement)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return;
            }

            if (!context.User.IsInRole("Funcionario"))
                return;

            var userId = context.User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return;

            var agora = DateTime.UtcNow;

            var temPermissaoAtiva = await _permissaoRepository
                .GetAllWithUsers()
                .AnyAsync(p =>
                    p.FuncionarioUserId == userId &&
                    !p.Revogada &&
                    p.DataInicio <= agora &&
                    p.DataFim > agora);

            if (temPermissaoAtiva)
                context.Succeed(requirement);
        }
    }
}
