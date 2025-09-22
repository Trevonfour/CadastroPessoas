using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StefaniniCadastroPessoas.DTOs;
using StefaniniCadastroPessoas.Services;

namespace StefaniniCadastroPessoas.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var resultado = await _authService.LoginAsync(loginDto);

                if (resultado == null)
                {
                    return Unauthorized(new { message = "Usuário ou senha inválidos" });
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint de login");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

      
        [HttpPost("registro")]
        [ProducesResponseType(typeof(AuthResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Registro([FromBody] RegistroDto registroDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var resultado = await _authService.RegistrarAsync(registroDto);

                if (resultado == null)
                {
                    return BadRequest(new { message = "Nome de usuário ou email já existem" });
                }

                return CreatedAtAction(nameof(ObterPerfil), new { }, resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint de registro");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost("recuperar-senha")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RecuperarSenha([FromBody] RecuperarSenhaDto recuperarSenhaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var sucesso = await _authService.RecuperarSenhaAsync(recuperarSenhaDto.Email);

                // Por segurança, sempre retornamos sucesso, mesmo se o email não existir
                return Ok(new { message = "Se o email existir, você receberá instruções para recuperação da senha" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint de recuperação de senha");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost("redefinir-senha")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaDto redefinirSenhaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var sucesso = await _authService.RedefinirSenhaAsync(redefinirSenhaDto);

                if (!sucesso)
                {
                    return BadRequest(new { message = "Token inválido ou expirado" });
                }

                return Ok(new { message = "Senha redefinida com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint de redefinição de senha");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        [HttpPost("alterar-senha")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto alterarSenhaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (usuarioId == 0)
                {
                    return Unauthorized(new { message = "Usuário não identificado" });
                }

                var sucesso = await _authService.AlterarSenhaAsync(usuarioId, alterarSenhaDto);

                if (!sucesso)
                {
                    return BadRequest(new { message = "Senha atual incorreta" });
                }

                return Ok(new { message = "Senha alterada com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint de alteração de senha");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

 
        [HttpGet("perfil")]
        [Authorize]
        [ProducesResponseType(typeof(UsuarioResponseDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ObterPerfil()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (usuarioId == 0)
                {
                    return Unauthorized(new { message = "Usuário não identificado" });
                }

                var usuario = await _authService.ObterUsuarioAsync(usuarioId);

                if (usuario == null)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint de perfil do usuário");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

 
        [HttpGet("validar-token")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult ValidarToken()
        {
            try
            {
                var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var nomeUsuario = User.FindFirst(ClaimTypes.Name)?.Value;

                return Ok(new 
                { 
                    valido = true, 
                    usuarioId = usuarioId,
                    nomeUsuario = nomeUsuario,
                    message = "Token válido" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint de validação de token");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

    
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        public IActionResult Logout()
        {
            try
            {
                return Ok(new { message = "Logout realizado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no endpoint de logout");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
    }
}
