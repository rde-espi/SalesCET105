using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedDb(DataContext context, RoleManager<IdentityRole> roleManager,UserManager<User> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync();
            await SeedRolesAsync();
            await SeedEstadosMarcacaoAsync();
            await SeedAdminAsync();
        }

        private async Task SeedAdminAsync()
        {
            var email = "r75312@gmail.com";
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    NomeCompleto = "Administrador",
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    Ativo = true,
                    DataCriacao = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, "#Admin123");


                if (!result.Succeeded)
                {
                    throw new Exception(
                        string.Join(";", result.Errors.Select(e => e.Description)));
                }
            }
            if(!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
        }

        private async Task SeedEstadosMarcacaoAsync()
        {
            if (!await _context.EstadosMarcacoes.AnyAsync())
            {
                var estados = new List<EstadoMarcacao> 
                {
                    new EstadoMarcacao
                    {
                        Nome="Pendente",
                        Descricao="A marcação aguarda confirmação"
                    },

                    new EstadoMarcacao
                    {
                        Nome="Confirmada",
                        Descricao="A marcação foi confirmada"
                    },

                    new EstadoMarcacao
                    {
                        Nome="Concluida",
                        Descricao="O serviço foi realizado"
                    },

                    new EstadoMarcacao
                    {
                        Nome="Cancelada",
                        Descricao="A marcação foi cancelada"
                    },

                    new EstadoMarcacao
                    {
                        Nome= "Não Compareceu",
                        Descricao="O cliente não compareceu à marcação"
                    }
                };
                await _context.EstadosMarcacoes.AddRangeAsync(estados);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedRolesAsync()
        {
            string[] roles = { "Admin", "Funcionario", "Cliente" };

            foreach (var role in roles)
            {
                if(!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
