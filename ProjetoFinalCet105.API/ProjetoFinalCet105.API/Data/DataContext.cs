using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Competencia> Competencias { get; set; }
        public DbSet<FuncionarioServico> FuncionariosServicos { get; set; }
        public DbSet<FuncionarioCompetencia> FuncionariosCompetencias { get; set; }
        public DbSet<HorarioFuncionario> HorariosFuncionarios { get; set; }
        public DbSet<Indisponibilidade> Indisponibilidades { get; set; }
        public DbSet<Marcacao> Marcacoes { get; set; }
        public DbSet<EstadoMarcacao> EstadosMarcacoes { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<HistoricoMarcacao> HistoricosMarcacoes { get; set; }
        public DbSet<Conversa> Conversas { get; set; }
        public DbSet<Mensagem> Mensagens { get; set; }
        public DbSet<DispositivoUser> DispositivosUsers { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<GoogleCalendarConta> GoogleCalendarContas { get; set; }
        public DbSet<GoogleCalendarEvento> GoogleCalendarEventos { get; set; }
        public DbSet<Fatura> Faturas { get; set; }
        public DbSet<FaturaItem> FaturaItens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {      
            base.OnModelCreating(modelBuilder);

            // SERVICO -> CATEGORIA
            modelBuilder.Entity<Servico>()
                .HasOne(s => s.Categoria)
                .WithMany(c => c.Servicos)
                .HasForeignKey(s => s.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);


            // FUNCIONARIO -> USER
            modelBuilder.Entity<Funcionario>()
                .HasOne(f => f.User)
                .WithOne()
                .HasForeignKey<Funcionario>(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // FUNCIONARIO SERVICO -> FUNCIONARIO
            modelBuilder.Entity<FuncionarioServico>()
                .HasOne(fs => fs.Funcionario)
                .WithMany(f => f.FuncionarioServicos)
                .HasForeignKey(fs => fs.FuncionarioId)
                .OnDelete(DeleteBehavior.Restrict);


            // FUNCIONARIO SERVICO -> SERVICO
            modelBuilder.Entity<FuncionarioServico>()
                .HasOne(fs => fs.Servico)
                .WithMany(s => s.FuncionarioServicos)
                .HasForeignKey(fs => fs.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);


            // FUNCIONARIO COMPETENCIA -> FUNCIONARIO
            modelBuilder.Entity<FuncionarioCompetencia>()
                .HasOne(fc => fc.Funcionario)
                .WithMany(f => f.FuncionarioCompetencias)
                .HasForeignKey(fc => fc.FuncionarioId)
                .OnDelete(DeleteBehavior.Restrict);


            // FUNCIONARIO COMPETENCIA -> COMPETENCIA
            modelBuilder.Entity<FuncionarioCompetencia>()
                .HasOne(fc => fc.Competencia)
                .WithMany(c => c.FuncionarioCompetencias)
                .HasForeignKey(fc => fc.CompetenciaId)
                .OnDelete(DeleteBehavior.Restrict);


            // HORARIO -> FUNCIONARIO
            modelBuilder.Entity<HorarioFuncionario>()
                .HasOne(h => h.Funcionario)
                .WithMany(f => f.Horarios)
                .HasForeignKey(h => h.FuncionarioId)
                .OnDelete(DeleteBehavior.Restrict);


            // INDISPONIBILIDADE -> FUNCIONARIO
            modelBuilder.Entity<Indisponibilidade>()
                .HasOne(i => i.Funcionario)
                .WithMany(f => f.Indisponibilidades)
                .HasForeignKey(i => i.FuncionarioId)
                .OnDelete(DeleteBehavior.Restrict);


            // MARCACAO -> CLIENTE (USER)
            modelBuilder.Entity<Marcacao>()
                .HasOne(m => m.Cliente)
                .WithMany()
                .HasForeignKey(m => m.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);


            // MARCACAO -> FUNCIONARIO
            modelBuilder.Entity<Marcacao>()
                .HasOne(m => m.Funcionario)
                .WithMany()
                .HasForeignKey(m => m.FuncionarioId)
                .OnDelete(DeleteBehavior.Restrict);


            // MARCACAO -> SERVICO
            modelBuilder.Entity<Marcacao>()
                .HasOne(m => m.Servico)
                .WithMany()
                .HasForeignKey(m => m.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);


            // MARCACAO -> ESTADO
            modelBuilder.Entity<Marcacao>()
                .HasOne(m => m.EstadoMarcacao)
                .WithMany(e => e.Marcacoes)
                .HasForeignKey(m => m.EstadoMarcacaoId)
                .OnDelete(DeleteBehavior.Restrict);
            // FEEDBACK -> MARCACAO
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Marcacao)
                .WithMany()
                .HasForeignKey(f => f.MarcacaoId)
                .OnDelete(DeleteBehavior.Restrict);

            // FEEDBACK -> CLIENTE (USER)
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Cliente)
                .WithMany()
                .HasForeignKey(f => f.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // FEEDBACK -> FUNCIONARIO
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Funcionario)
                .WithMany()
                .HasForeignKey(f => f.FuncionarioId)
                .OnDelete(DeleteBehavior.Restrict);


            // NOTIFICACAO -> USER
            modelBuilder.Entity<Notificacao>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // HISTORICO MARCACAO -> MARCACAO
            modelBuilder.Entity<HistoricoMarcacao>()
                .HasOne(h => h.Marcacao)
                .WithMany()
                .HasForeignKey(h => h.MarcacaoId)
                .OnDelete(DeleteBehavior.Restrict);

            // HISTORICO MARCACAO -> USER
            modelBuilder.Entity<HistoricoMarcacao>()
                .HasOne(h => h.User)
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // CONVERSA -> CLIENTE
            modelBuilder.Entity<Conversa>()
                .HasOne(c => c.Cliente)
                .WithMany()
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // CONVERSA -> FUNCIONARIO
            modelBuilder.Entity<Conversa>()
                .HasOne(c => c.Funcionario)
                .WithMany()
                .HasForeignKey(c => c.FuncionarioUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // MENSAGEM -> CONVERSA
            modelBuilder.Entity<Mensagem>()
                .HasOne(m => m.Conversa)
                .WithMany(c => c.Mensagens)
                .HasForeignKey(m => m.ConversaId)
                .OnDelete(DeleteBehavior.Restrict);

            // MENSAGEM -> REMETENTE (USER)
            modelBuilder.Entity<Mensagem>()
                .HasOne(m => m.Remetente)
                .WithMany()
                .HasForeignKey(m => m.RemetenteId)
                .OnDelete(DeleteBehavior.Restrict);
            // GOOGLE CALENDAR CONTA -> USER
            modelBuilder.Entity<GoogleCalendarConta>()
                .HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Um utilizador só pode ter uma ligação Google Calendar
            modelBuilder.Entity<GoogleCalendarConta>()
                .HasIndex(g => g.UserId)
                .IsUnique();


            // GOOGLE CALENDAR EVENTO -> MARCACAO
            modelBuilder.Entity<GoogleCalendarEvento>()
                .HasOne(g => g.Marcacao)
                .WithMany()
                .HasForeignKey(g => g.MarcacaoId)
                .OnDelete(DeleteBehavior.Restrict);

            // GOOGLE CALENDAR EVENTO -> USER
            modelBuilder.Entity<GoogleCalendarEvento>()
                .HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Impede duplicar o mesmo evento Google
            // para a mesma marcação/utilizador
            modelBuilder.Entity<GoogleCalendarEvento>()
                .HasIndex(g => new
                {
                    g.MarcacaoId,
                    g.UserId
                })
                .IsUnique();

            // FATURA -> MARCACAO
            modelBuilder.Entity<Fatura>()
                .HasOne(f => f.Marcacao)
                .WithMany()
                .HasForeignKey(f => f.MarcacaoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Fatura>()
                .HasIndex(f => f.MarcacaoId)
                .IsUnique();

            // FATURA ITEM -> FATURA
            modelBuilder.Entity<FaturaItem>()
                .HasOne(fi => fi.Fatura)
                .WithMany(f => f.Itens)
                .HasForeignKey(fi => fi.FaturaId)
                .OnDelete(DeleteBehavior.Cascade);

            // PRECISÃO DECIMAL - FATURA
            modelBuilder.Entity<Fatura>()
                .Property(f => f.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Fatura>()
                .Property(f => f.ValorDesconto)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Fatura>()
                .Property(f => f.ValorIva)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Fatura>()
                .Property(f => f.Total)
                .HasPrecision(18, 2);
            // Número completo da fatura deve ser único
            modelBuilder.Entity<Fatura>()
                .HasIndex(f => f.Numero)
                .IsUnique();

            // PRECISÃO DECIMAL - FATURA ITEM
            modelBuilder.Entity<FaturaItem>()
                .Property(fi => fi.Quantidade)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FaturaItem>()
                .Property(fi => fi.PrecoUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FaturaItem>()
                .Property(fi => fi.PercentagemIva)
                .HasPrecision(5, 2);

            modelBuilder.Entity<FaturaItem>()
                .Property(fi => fi.ValorIva)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FaturaItem>()
                .Property(fi => fi.Total)
                .HasPrecision(18, 2);
            // Série + número sequencial devem ser únicos
            modelBuilder.Entity<Fatura>()
                .HasIndex(f => new { f.Serie, f.NumeroSequencial })
                .IsUnique();
        }

    }
}
