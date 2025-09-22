using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StefaniniCadastroPessoas.Controllers;
using StefaniniCadastroPessoas.Services;
using StefaniniCadastroPessoas.DTOs;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace StefaniniCadastroPessoas.Tests
{
    /// <summary>
    /// Testes de unidade para a classe AuthController.
    /// </summary>
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
        }

        // --- Testes para Login ---

        [Fact]
        public async Task Login_ComCredenciaisValidas_DeveRetornarOkComToken()
        {
            // Arrange
            var loginDto = new LoginDto { Usuario = "test", Senha = "password" };
            var authResponse = new AuthResponseDto { Token = "jwt-token" };
            _mockAuthService.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(authResponse, okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { Usuario = "test", Senha = "wrong" };
            _mockAuthService.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync((AuthResponseDto?)null);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }
        
        [Fact]
        public async Task Login_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto();
            _controller.ModelState.AddModelError("Error", "Sample error");

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        // --- Testes para Registro ---
        
        [Fact]
        public async Task Registro_ComDadosValidos_DeveRetornarCreatedAtAction()
        {
            // Arrange
            var registroDto = new RegistroDto { NomeUsuario = "newuser", Email = "new@user.com", Senha = "p", ConfirmarSenha = "p" };
            var authResponse = new AuthResponseDto { Token = "jwt-token" };
            _mockAuthService.Setup(s => s.RegistrarAsync(registroDto)).ReturnsAsync(authResponse);

            // Act
            var result = await _controller.Registro(registroDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(AuthController.ObterPerfil), createdAtActionResult.ActionName);
            Assert.Equal(201, createdAtActionResult.StatusCode);
        }
        
        [Fact]
        public async Task Registro_ComUsuarioExistente_DeveRetornarBadRequest()
        {
            // Arrange
            var registroDto = new RegistroDto { NomeUsuario = "existinguser" };
            _mockAuthService.Setup(s => s.RegistrarAsync(registroDto)).ReturnsAsync((AuthResponseDto?)null);
            
            // Act
            var result = await _controller.Registro(registroDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        // --- Testes para ObterPerfil ---

        [Fact]
        public async Task ObterPerfil_ComUsuarioAutenticado_DeveRetornarOkComUsuario()
        {
            // Arrange
            var usuarioId = "1";
            var expectedUser = new UsuarioResponseDto { Id = 1, NomeUsuario = "testuser" };
            _mockAuthService.Setup(s => s.ObterUsuarioAsync(1)).ReturnsAsync(expectedUser);

            var controller = CreateControllerWithUser(usuarioId, "testuser");
            
            // Act
            var result = await controller.ObterPerfil();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedUser, okResult.Value);
        }
        
        [Fact]
        public async Task ObterPerfil_ComUsuarioNaoEncontrado_DeveRetornarNotFound()
        {
            // Arrange
            var usuarioId = "1";
            _mockAuthService.Setup(s => s.ObterUsuarioAsync(1)).ReturnsAsync((UsuarioResponseDto?)null);
            
            var controller = CreateControllerWithUser(usuarioId, "testuser");
            
            // Act
            var result = await controller.ObterPerfil();
            
            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
        
        [Fact]
        public async Task ObterPerfil_SemClaimDeId_DeveRetornarUnauthorized()
        {
            // Arrange
            // Cria um controller sem o claim de ID de usuário
            var controller = CreateControllerWithUser(null, "testuser");
            
            // Act
            var result = await controller.ObterPerfil();
            
            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
        
        // --- Testes para AlterarSenha ---

        [Fact]
        public async Task AlterarSenha_ComDadosValidos_DeveRetornarOk()
        {
            // Arrange
            var usuarioId = "1";
            var alterarSenhaDto = new AlterarSenhaDto { SenhaAtual = "old", NovaSenha = "new", ConfirmarNovaSenha = "new" };
            _mockAuthService.Setup(s => s.AlterarSenhaAsync(1, alterarSenhaDto)).ReturnsAsync(true);
            
            var controller = CreateControllerWithUser(usuarioId, "testuser");
            
            // Act
            var result = await controller.AlterarSenha(alterarSenhaDto);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task AlterarSenha_ComSenhaAtualIncorreta_DeveRetornarBadRequest()
        {
            // Arrange
            var usuarioId = "1";
            var alterarSenhaDto = new AlterarSenhaDto { SenhaAtual = "wrong", NovaSenha = "new", ConfirmarNovaSenha = "new" };
            _mockAuthService.Setup(s => s.AlterarSenhaAsync(1, alterarSenhaDto)).ReturnsAsync(false);
            
            var controller = CreateControllerWithUser(usuarioId, "testuser");
            
            // Act
            var result = await controller.AlterarSenha(alterarSenhaDto);
            
            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
        
        /// <summary>
        /// Método auxiliar para criar uma instância do AuthController com um usuário autenticado simulado.
        /// </summary>
        private AuthController CreateControllerWithUser(string? userId, string? userName)
        {
            var claims = new List<Claim>();
            if (userId != null)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            if (userName != null)
                claims.Add(new Claim(ClaimTypes.Name, userName));

            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            
            var controller = new AuthController(_mockAuthService.Object, _mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };
            return controller;
        }
    }
}
