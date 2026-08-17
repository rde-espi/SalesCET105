
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;


var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddDbContext<DataContext>(
                options => options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnections"),
                    sqlServerOptionsAction: sqlOptions =>                    
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null
                            )));

            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<DataContext>()
                .AddDefaultTokenProviders();           
            builder.Services.AddSwaggerGen();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddScoped<SeedDb>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICompetenciaRepository, CompetenciaRepository>();
builder.Services.AddScoped<IConversaRepository, ConversaRepository>();
builder.Services.AddScoped<IEstadoMarcacaoRepository, EstadoMarcacaoRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
builder.Services.AddScoped<IFuncionarioCompetenciaRepository, FuncionarioCompetenciaRepository>();
builder.Services.AddScoped<IFuncionarioServicoRepository, FuncionarioServicoRepository>();
builder.Services.AddScoped<IHistoricoMarcacaoRepository, HistoricoMarcacaoRepository>();
builder.Services.AddScoped<IHorarioFuncionarioRepository, HorarioFuncionarioRepository>();
builder.Services.AddScoped<IIndisponibilidadeRepository, IndisponibilidadeRepository>();
builder.Services.AddScoped<IMarcacaoRepository, MarcacaoRepository>();
builder.Services.AddScoped<IMensagemRepository, MensagemRepository>();
builder.Services.AddScoped<INotificacaoRepository,NotificacaoRepository>();
builder.Services.AddScoped<IPromoCodeRepository, PromoCodeRepository>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();

            var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seedDb = scope.ServiceProvider.GetRequiredService<SeedDb>();
    await seedDb.SeedAsync();
}

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json","Projeto Final API"));
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        