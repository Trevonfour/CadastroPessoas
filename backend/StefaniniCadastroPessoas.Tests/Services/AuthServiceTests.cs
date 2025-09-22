using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StefaniniCadastroPessoas.Data;
using StefaniniCadastroPessoas.Models;
using StefaniniCadastroPessoas.Services;
using StefaniniCadastroPessoas.DTOs;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;

namespace StefaniniCadastroPessoas.Tests
{
    /// <summary>
    /// Testes de unidade para a classe AuthService, com foco em 100% de cobertura.
    /// </summary>
    public class AuthServiceTests : IDisposable
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public AuthServiceTests()
        {
            // Configuração do banco de dados em memória
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Mock da Configuração para JWT
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("StefaniniSuperSecretKey2024ForTestingPurpose");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            // Mock do Logger
            _mockLogger = new Mock<ILogger<AuthService>>();
        }

        // --- Testes para LoginAsync ---

        [Fact]
        public async Task LoginAsync_ComCredenciaisValidas_DeveRetornarAuthResponse()
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var senha = "Password123!";
            var usuario = new Usuario { Id = 1, NomeUsuario = "testuser", Email = "test@user.com", SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha), Ativo = true, NomeCompleto = "Test User", Perfil = "User" };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var loginDto = new LoginDto { Usuario = "testuser", Senha = senha };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.Equal("testuser", result.Usuario?.NomeUsuario);

            // Verifica se o último login foi atualizado
            var userInDb = await context.Usuarios.FindAsync(1);
            Assert.NotNull(userInDb?.UltimoLogin);
        }

        [Theory]
        [InlineData("usuario_inexistente", "senhaqualquer")]
        [InlineData("testuser", "senha_errada")]
        public async Task LoginAsync_ComCredenciaisInvalidas_DeveRetornarNull(string username, string password)
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var usuario = new Usuario { Id = 1, NomeUsuario = "testuser", SenhaHash = BCrypt.Net.BCrypt.HashPassword("senha_correta"), Ativo = true };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var loginDto = new LoginDto { Usuario = username, Senha = password };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ComUsuarioInativo_DeveRetornarNull()
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var senha = "Password123!";
            var usuario = new Usuario { Id = 1, NomeUsuario = "inactiveuser", SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha), Ativo = false };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var loginDto = new LoginDto { Usuario = "inactiveuser", Senha = senha };

            // Act
            var result = await service.LoginAsync(loginDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_QuandoOcorreExcecao_DeveRetornarNullELogarErro()
        {
            // Arrange
            var senha = "Password123!";
            var usuario = new Usuario { Id = 1, NomeUsuario = "testuser", SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha), Ativo = true };

            // Passo 1: Use um contexto normal para preparar (seed) o banco de dados em memória
            using (var seedContext = new TestApplicationDbContext(_dbOptions))
            {
                seedContext.Usuarios.Add(usuario);
                await seedContext.SaveChangesAsync();
            }

            // Passo 2: Use o contexto que lança exceção para a ação de teste
            using (var exceptionContext = new ExceptionThrowingDbContext(_dbOptions))
            {
                var service = new AuthService(exceptionContext, _mockConfiguration.Object, _mockLogger.Object);
                var loginDto = new LoginDto { Usuario = "testuser", Senha = senha };

                // Act
                var result = await service.LoginAsync(loginDto);

                // Assert
                Assert.Null(result);
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao realizar login")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
        }


        // --- Testes para RegistrarAsync ---

        [Fact]
        public async Task RegistrarAsync_ComDadosValidos_DeveCriarUsuarioERetornarAuthResponse()
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var registroDto = new RegistroDto
            {
                NomeUsuario = "newuser",
                Email = "new@user.com",
                NomeCompleto = "New User Full",
                Senha = "Password123!",
                ConfirmarSenha = "Password123!"
            };

            // Act
            var result = await service.RegistrarAsync(registroDto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.Equal("newuser", result.Usuario?.NomeUsuario);
            var userInDb = await context.Usuarios.FirstOrDefaultAsync(u => u.NomeUsuario == "newuser");
            Assert.NotNull(userInDb);
            Assert.Equal("User", userInDb.Perfil);
        }

        [Theory]
        [InlineData("existinguser", "new@email.com")] // Usuário duplicado
        [InlineData("newuser", "existing@email.com")] // Email duplicado
        public async Task RegistrarAsync_ComUsuarioOuEmailDuplicado_DeveRetornarNull(string username, string email)
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var usuarioExistente = new Usuario { Id = 1, NomeUsuario = "existinguser", Email = "existing@email.com", SenhaHash = "hash" };
            context.Usuarios.Add(usuarioExistente);
            await context.SaveChangesAsync();

            var registroDto = new RegistroDto { NomeUsuario = username, Email = email, Senha = "p", ConfirmarSenha = "p", NomeCompleto = "n" };

            // Act
            var result = await service.RegistrarAsync(registroDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RegistrarAsync_QuandoOcorreExcecao_DeveRetornarNullELogarErro()
        {
            // Arrange
            using var context = new ExceptionThrowingDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var registroDto = new RegistroDto { NomeUsuario = "any", Email = "any@any.com", Senha = "p", ConfirmarSenha = "p", NomeCompleto = "n" };

            // Act
            var result = await service.RegistrarAsync(registroDto);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
               x => x.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao registrar usuário")),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Once);
        }

        // --- Testes para ObterUsuarioAsync ---

        [Fact]
        public async Task ObterUsuarioAsync_ComIdValidoEAtivo_DeveRetornarUsuario()
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var usuario = new Usuario { Id = 1, NomeUsuario = "testuser", Email = "test@user.com", Ativo = true, NomeCompleto = "Test User" };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            // Act
            var result = await service.ObterUsuarioAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("testuser", result.NomeUsuario);
        }

        [Theory]
        [InlineData(99)] // ID Inexistente
        [InlineData(2)]  // ID Inativo
        public async Task ObterUsuarioAsync_ComIdInvalidoOuInativo_DeveRetornarNull(int userId)
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var usuarioInativo = new Usuario { Id = 2, NomeUsuario = "inactive", Ativo = false };
            context.Usuarios.Add(usuarioInativo);
            await context.SaveChangesAsync();

            // Act
            var result = await service.ObterUsuarioAsync(userId);

            // Assert
            Assert.Null(result);
        }

        // --- Testes para AlterarSenhaAsync ---

        [Fact]
        public async Task AlterarSenhaAsync_ComDadosValidos_DeveRetornarTrueEAlterarSenha()
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var senhaAntiga = "SenhaAntiga123!";
            var senhaNova = "SenhaNova456!";
            var senhaHashAntiga = BCrypt.Net.BCrypt.HashPassword(senhaAntiga);
            var usuario = new Usuario { Id = 1, NomeUsuario = "testuser", SenhaHash = senhaHashAntiga, Ativo = true };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var alterarSenhaDto = new AlterarSenhaDto { SenhaAtual = senhaAntiga, NovaSenha = senhaNova, ConfirmarNovaSenha = senhaNova };

            // Act
            var result = await service.AlterarSenhaAsync(1, alterarSenhaDto);

            // Assert
            Assert.True(result);
            var userInDb = await context.Usuarios.FindAsync(1);
            Assert.NotNull(userInDb);
            Assert.NotEqual(senhaHashAntiga, userInDb.SenhaHash);
            Assert.True(BCrypt.Net.BCrypt.Verify(senhaNova, userInDb.SenhaHash));
        }

        [Fact]
        public async Task AlterarSenhaAsync_ComSenhaAtualIncorreta_DeveRetornarFalse()
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var senhaCorreta = "SenhaCorreta123!";
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaCorreta);
            var usuario = new Usuario { Id = 1, NomeUsuario = "testuser", SenhaHash = senhaHash, Ativo = true };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var alterarSenhaDto = new AlterarSenhaDto { SenhaAtual = "senha_errada", NovaSenha = "nova", ConfirmarNovaSenha = "nova" };

            // Act
            var result = await service.AlterarSenhaAsync(1, alterarSenhaDto);

            // Assert
            Assert.False(result);
            var userInDb = await context.Usuarios.FindAsync(1);
            Assert.NotNull(userInDb);
            Assert.Equal(senhaHash, userInDb.SenhaHash); // Garante que a senha não mudou
        }

        [Fact]
        public async Task AlterarSenhaAsync_ComUsuarioInexistente_DeveRetornarFalse()
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var alterarSenhaDto = new AlterarSenhaDto { SenhaAtual = "a", NovaSenha = "b", ConfirmarNovaSenha = "b" };

            // Act
            var result = await service.AlterarSenhaAsync(99, alterarSenhaDto);

            // Assert
            Assert.False(result);
        }

        // --- Testes para Recuperar e Redefinir Senha ---

        [Fact]
        public async Task RecuperarSenhaAsync_ComEmailValido_DeveRetornarTrueEAtualizarUsuario()
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var usuario = new Usuario { Id = 1, Email = "test@user.com", Ativo = true };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            // Act
            var result = await service.RecuperarSenhaAsync("test@user.com");

            // Assert
            Assert.True(result);
            var userInDb = await context.Usuarios.FindAsync(1);
            Assert.NotNull(userInDb);
            Assert.NotNull(userInDb.TokenRecuperacaoSenha);
            Assert.NotNull(userInDb.TokenRecuperacaoExpiracao);
        }

        [Fact]
        public async Task RecuperarSenhaAsync_ComEmailInvalido_DeveRetornarFalse()
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);

            // Act
            var result = await service.RecuperarSenhaAsync("nonexistent@email.com");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RedefinirSenhaAsync_ComTokenValido_DeveRetornarTrueERedefinirSenha()
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var senhaNova = "NovaSenhaDefinida!";
            var usuario = new Usuario { Id = 1, Ativo = true, TokenRecuperacaoSenha = "valid-token", TokenRecuperacaoExpiracao = DateTime.UtcNow.AddHours(1) };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var redefinirDto = new RedefinirSenhaDto { Token = "valid-token", NovaSenha = senhaNova, ConfirmarNovaSenha = senhaNova };

            // Act
            var result = await service.RedefinirSenhaAsync(redefinirDto);

            // Assert
            Assert.True(result);
            var userInDb = await context.Usuarios.FindAsync(1);
            Assert.NotNull(userInDb);
            Assert.True(BCrypt.Net.BCrypt.Verify(senhaNova, userInDb.SenhaHash));
            Assert.Null(userInDb.TokenRecuperacaoSenha);
            Assert.Null(userInDb.TokenRecuperacaoExpiracao);
        }

        [Theory]
        [InlineData("invalid-token")] // Token errado
        [InlineData("expired-token")] // Token expirado
        public async Task RedefinirSenhaAsync_ComTokenInvalidoOuExpirado_DeveRetornarFalse(string token)
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var usuario = new Usuario { Id = 1, Ativo = true, TokenRecuperacaoSenha = "expired-token", TokenRecuperacaoExpiracao = DateTime.UtcNow.AddMinutes(-1) };
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();

            var redefinirDto = new RedefinirSenhaDto { Token = token, NovaSenha = "nova", ConfirmarNovaSenha = "nova" };

            // Act
            var result = await service.RedefinirSenhaAsync(redefinirDto);

            // Assert
            Assert.False(result);
        }

        // --- Teste para GerarToken ---

        [Fact]
        public void GerarToken_ComUsuarioValido_DeveRetornarTokenComClaimsCorretas()
        {
            // Arrange
            using var context = new TestApplicationDbContext(_dbOptions);
            var service = new AuthService(context, _mockConfiguration.Object, _mockLogger.Object);
            var usuario = new Usuario
            {
                Id = 123,
                NomeUsuario = "tokenuser",
                Email = "token@user.com",
                Perfil = "Admin",
                NomeCompleto = "Token User Full"
            };

            // Act
            var tokenString = service.GerarToken(usuario);

            // Assert
            Assert.NotNull(tokenString);
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenString);

            Assert.Equal("123", token.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal("tokenuser", token.Claims.First(c => c.Type == ClaimTypes.Name).Value);
            Assert.Equal("token@user.com", token.Claims.First(c => c.Type == ClaimTypes.Email).Value);
            Assert.Equal("Admin", token.Claims.First(c => c.Type == ClaimTypes.Role).Value);
            Assert.Equal("Token User Full", token.Claims.First(c => c.Type == "NomeCompleto").Value);
            Assert.Equal("TestIssuer", token.Issuer);
            Assert.Equal("TestAudience", token.Audiences.First());
        }

        public void Dispose()
        {
            // Nenhuma limpeza necessária por instância, pois o DbContext é gerenciado por teste.
        }
    }

    /// <summary>
    /// DbContext de teste que herda do DbContext real para usar o mesmo modelo,
    /// mas sobrescreve o SaveChangesAsync para evitar lógicas customizadas que
    /// possam interferir com os testes de unidade.
    /// </summary>
    public class TestApplicationDbContext : ApplicationDbContext
    {
        public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Chama a implementação base do DbContext, ignorando a override do ApplicationDbContext
            return base.SaveChangesAsync(true, cancellationToken);
        }
    }

    /// <summary>
    /// DbContext de teste projetado para lançar uma exceção ao salvar,
    /// permitindo testar os blocos catch de tratamento de erro.
    /// </summary>
    public class ExceptionThrowingDbContext : ApplicationDbContext
    {
        public ExceptionThrowingDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new DbUpdateException("Erro de teste simulado ao salvar no banco de dados.");
        }
    }
}

