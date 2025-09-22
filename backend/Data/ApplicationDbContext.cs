using Microsoft.EntityFrameworkCore;
using StefaniniCadastroPessoas.Models;

namespace StefaniniCadastroPessoas.Data
{
    /// <summary>
    /// Contexto do banco de dados da aplicação
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// DbSet de pessoas
        /// </summary>
        public DbSet<Pessoa> Pessoas { get; set; }

        /// <summary>
        /// DbSet de usuários
        /// </summary>
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Pessoa>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CPF).IsRequired().HasMaxLength(14);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Sexo).HasMaxLength(1);
                entity.Property(e => e.Naturalidade).HasMaxLength(100);
                entity.Property(e => e.Nacionalidade).HasMaxLength(100);
                entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.DataAtualizacao).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Ativo).HasDefaultValue(true);

                entity.HasIndex(e => e.CPF).IsUnique().HasDatabaseName("IX_Pessoa_CPF");
                entity.HasIndex(e => e.Email).HasDatabaseName("IX_Pessoa_Email");
                entity.HasIndex(e => e.Nome).HasDatabaseName("IX_Pessoa_Nome");
            });
            
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NomeUsuario).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SenhaHash).IsRequired();
                entity.Property(e => e.NomeCompleto).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DataCriacao).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Ativo).HasDefaultValue(true);
                entity.Property(e => e.Perfil).HasMaxLength(20).HasDefaultValue("User");

                // Índices únicos
                entity.HasIndex(e => e.NomeUsuario).IsUnique().HasDatabaseName("IX_Usuario_NomeUsuario");
                entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("IX_Usuario_Email");
            });

            SeedData(modelBuilder);
        }

        /// <summary>
        /// Configura dados iniciais para o banco
        /// </summary>
        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    NomeUsuario = "admin",
                    Email = "admin@stefanini.com",
                    NomeCompleto = "Administrador do Sistema",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Perfil = "Admin",
                    DataCriacao = DateTime.UtcNow,
                    Ativo = true
                }
            );

            modelBuilder.Entity<Pessoa>().HasData(
                new Pessoa
                {
                    Id = 1,
                    Nome = "João Silva Santos",
                    CPF = "12345678901",
                    Email = "joao.silva@email.com",
                    DataNascimento = new DateTime(1990, 5, 15),
                    Sexo = "M",
                    Naturalidade = "São Paulo",
                    Nacionalidade = "Brasileira",
                    DataCadastro = DateTime.UtcNow,
                    DataAtualizacao = DateTime.UtcNow,
                    Ativo = true
                },
                new Pessoa
                {
                    Id = 2,
                    Nome = "Maria Oliveira Costa",
                    CPF = "98765432109",
                    Email = "maria.oliveira@email.com",
                    DataNascimento = new DateTime(1985, 8, 22),
                    Sexo = "F",
                    Naturalidade = "Rio de Janeiro",
                    Nacionalidade = "Brasileira",
                    DataCadastro = DateTime.UtcNow,
                    DataAtualizacao = DateTime.UtcNow,
                    Ativo = true
                },
                new Pessoa
                {
                    Id = 3,
                    Nome = "Carlos Eduardo Ferreira",
                    CPF = "11122233344",
                    Email = "carlos.ferreira@email.com",
                    DataNascimento = new DateTime(1992, 12, 3),
                    Sexo = "M",
                    Naturalidade = "Belo Horizonte",
                    Nacionalidade = "Brasileira",
                    DataCadastro = DateTime.UtcNow,
                    DataAtualizacao = DateTime.UtcNow,
                    Ativo = true
                }
            );
        }

        /// <summary>
        /// Override do SaveChanges para atualizar automaticamente a data de atualização
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override do SaveChangesAsync para atualizar automaticamente a data de atualização
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Atualiza automaticamente os timestamps de entidades modificadas
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Pessoa pessoa)
                {
                    pessoa.DataAtualizacao = DateTime.UtcNow;
                }
            }
        }
    }
}
