using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using ProjetoFinalCet105.API.Services.AdminTemporario;

namespace ProjetoFinalCet105.API.Authorization
{
    public class AdminOuAdminTemporarioHandler
         : AuthorizationHandler<AdminOuAdminTemporarioRequirement>
    {
        private readonly AdminTemporarioService _adminTemporarioService;

        public AdminOuAdminTemporarioHandler(AdminTemporarioService adminTemporarioService)
        {
            _adminTemporarioService = adminTemporarioService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOuAdminTemporarioRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
                return;

            // Administrador permanente
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return;
            }

            // Apenas Funcionário pode ter acesso administrativo temporário
            if (!context.User.IsInRole("Funcionario"))
                return;

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return;

            if (await _adminTemporarioService.TemPermissaoAtivaAsync(userId))
                context.Succeed(requirement);
        }
    }
}