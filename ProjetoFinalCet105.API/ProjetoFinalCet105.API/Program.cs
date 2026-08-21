
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using System.Text;


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
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(
        builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GerirMarcacoes", policy =>
        policy.RequireRole("Funcionario", "Admin"));

    options.AddPolicy("CriarMarcacao", policy =>
        policy.RequireRole("Cliente", "Funcionario", "Admin"));

    options.AddPolicy("ConsultarAgenda", policy =>
        policy.RequireRole("Funcionario", "Admin"));

    options.AddPolicy("AlterarMarcacao", policy =>
    policy.RequireRole("Cliente", "Funcionario", "Admin"));

    options.AddPolicy("ConsultarMarcacoes", policy =>
    policy.RequireRole("Cliente", "Funcionario", "Admin"));

    options.AddPolicy("AdminOnly", policy =>
    policy.RequireRole("Admin"));

    options.AddPolicy("CancelarMarcacao", policy =>
    policy.RequireRole("Cliente", "Funcionario", "Admin"));
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Projeto Final API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Introduza o token JWT."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

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
builder.Services.AddScoped<INotificacaoRepository, NotificacaoRepository>();
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
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Projeto Final API"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
