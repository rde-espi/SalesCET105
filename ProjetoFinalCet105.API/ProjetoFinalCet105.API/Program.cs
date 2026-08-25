
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ProjetoFinalCet105.API.Data;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.AuthService;
using ProjetoFinalCet105.API.Services.EmailService;
using ProjetoFinalCet105.API.Services.FirebaseService;
using ProjetoFinalCet105.API.Services.HorarioFuncionarioService;
using ProjetoFinalCet105.API.Services.IndisponibilidadeService;
using ProjetoFinalCet105.API.Services.MarcacaoService;
using ProjetoFinalCet105.API.Services.NotificacaoService;
using ProjetoFinalCet105.API.UseCases.AuthUsecase;
using ProjetoFinalCet105.API.UseCases.Cliente;
using ProjetoFinalCet105.API.UseCases.Conversas;
using ProjetoFinalCet105.API.UseCases.Conversas.SignalR.Hubs;
using ProjetoFinalCet105.API.UseCases.Feedbacks;
using ProjetoFinalCet105.API.UseCases.Funcionarios;
using ProjetoFinalCet105.API.UseCases.HorariosFuncionarios;
using ProjetoFinalCet105.API.UseCases.Indisponibilidades;
using ProjetoFinalCet105.API.UseCases.Marcacoes;
using ProjetoFinalCet105.API.UseCases.Notificacoes;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

var firebaseCredentialsPath = builder.Configuration["Firebase:CredentialsPath"];

if (string.IsNullOrWhiteSpace(firebaseCredentialsPath))
{
    throw new InvalidOperationException(
        "As credenciais do Firebase não estão configuradas.");
}

var credential = CredentialFactory
        .FromFile<ServiceAccountCredential>(firebaseCredentialsPath)
        .ToGoogleCredential();

FirebaseApp.Create(new AppOptions
{
    Credential = credential
});

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

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken =
                context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/chat"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
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

    options.AddPolicy("FeedbackMarcação", policy =>
    policy.RequireRole("Cliente", "Admin"));

    options.AddPolicy("ConsultarIndisponibilidades", policy =>
    policy.RequireRole("Funcionario", "Admin"));

    options.AddPolicy("GerirIndisponibilidades", policy =>
        policy.RequireRole("Funcionario", "Admin"));

    options.AddPolicy("AlterarFuncionario", policy =>
    policy.RequireRole("Funcionario", "Admin"));

    options.AddPolicy("ConsultarCliente", policy =>
    policy.RequireRole("Cliente", "Admin"));

    options.AddPolicy("AlterarCliente", policy =>
    policy.RequireRole("Cliente", "Admin"));

    options.AddPolicy("ConsultarHorario", policy =>
    policy.RequireRole("Funcionario", "Admin"));

    options.AddPolicy("GerirHorario", policy =>
        policy.RequireRole("Funcionario", "Admin"));

    options.AddPolicy("GerirCompetenciasFuncionario", policy =>
    policy.RequireRole("Funcionario", "Admin"));
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

//Repositories
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
builder.Services.AddScoped<IDispositivoUserRepository,DispositivoUserRepository>();




//Services
builder.Services.AddScoped<IMarcacaoService, MarcacaoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IIndisponibilidadeService, IndisponibilidadeService>();
builder.Services.AddScoped<IHorarioFuncionarioService, HorarioFuncionarioService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificacaoService, NotificacaoService>();
builder.Services.AddHostedService<LembreteMarcacoesBackgroundService>();
builder.Services.AddScoped<IFirebaseService,FirebaseService>();
builder.Services.AddSignalR();

//UseCases
builder.Services.AddScoped<CreateFeedbackUseCase>();
builder.Services.AddScoped<CreateMarcacaoUseCase>();
builder.Services.AddScoped<UpdateMarcacaoUseCase>();
builder.Services.AddScoped<CancelarMarcacaoUseCase>();
builder.Services.AddScoped<UpdateEstadoMarcacaoUseCase>();
builder.Services.AddScoped<GetDisponibilidadeUseCase>();
builder.Services.AddScoped<GetFeedbackByIdUseCase>();
builder.Services.AddScoped<GetFeedbacksByFuncionarioUseCase>();
builder.Services.AddScoped<GetFeedbackResumoFuncionarioUseCase>();
builder.Services.AddScoped<UpdateFeedbackUseCase>();
builder.Services.AddScoped<DeleteFeedbackUseCase>();
builder.Services.AddScoped<CreateIndisponibilidadeUseCase>();
builder.Services.AddScoped<UpdateIndisponibilidadeUseCase>();
builder.Services.AddScoped<DeleteIndisponibilidadeUseCase>();
builder.Services.AddScoped<CreateFuncionarioUseCase>();
builder.Services.AddScoped<UpdateFuncionarioUseCase>();
builder.Services.AddScoped<DeleteFuncionarioUseCase>();
builder.Services.AddScoped<CreateClienteUseCase>();
builder.Services.AddScoped<UpdateClienteUseCase>();
builder.Services.AddScoped<CreateHorarioFuncionarioUseCase>();
builder.Services.AddScoped<UpdateHorarioFuncionarioUseCase>();
builder.Services.AddScoped<DeleteHorarioFuncionarioUseCase>();
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<AlterarPasswordUseCase>();
builder.Services.AddScoped<RecuperarPasswordUseCase>();
builder.Services.AddScoped<ResetPasswordUseCase>();
builder.Services.AddScoped<VerificarTwoFactorUseCase>();
builder.Services.AddScoped<GerirTwoFactorUseCase>();
builder.Services.AddScoped<ConfirmarEmailUseCase>();
builder.Services.AddScoped<ReenviarConfirmacaoEmailUseCase>();
builder.Services.AddScoped<MarcarNotificacaoLidaUseCase>();
builder.Services.AddScoped<MarcarTodasComoLidasUseCase>();
builder.Services.AddScoped<EnviarMensagemUseCase>();
builder.Services.AddScoped<CriarConversaUseCase>();
builder.Services.AddScoped<GetMinhasConversasUseCase>();
builder.Services.AddScoped<GetConversaByIdUseCase>();
builder.Services.AddScoped<MarcarMensagensComoLidasUseCase>();




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
app.MapHub<ChatHub>("/hubs/chat");
app.Run();
