using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using StefaniniCadastroPessoas.Data;
using StefaniniCadastroPessoas.Models;
using StefaniniCadastroPessoas.DTOs;

namespace StefaniniCadastroPessoas.Services
{
    /// <summary>
    /// Interface para o serviço de autenticação
    /// </summary>
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto?> RegistrarAsync(RegistroDto registroDto);
        Task<bool> RecuperarSenhaAsync(string email);
        Task<bool> RedefinirSenhaAsync(RedefinirSenhaDto redefinirSenhaDto);
        Task<bool> AlterarSenhaAsync(int usuarioId, AlterarSenhaDto alterarSenhaDto);
        Task<UsuarioResponseDto?> ObterUsuarioAsync(int usuarioId);
        string GerarToken(Usuario usuario);
    }

    /// <summary>
    /// Serviço de autenticação e gerenciamento de usuários
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Realiza o login do usuário
        /// </summary>
        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            try
            {
                // Busca o usuário por nome de usuário ou email
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => 
                        (u.NomeUsuario == loginDto.Usuario || u.Email == loginDto.Usuario) 
                        && u.Ativo);

                if (usuario == null)
                {
                    _logger.LogWarning("Tentativa de login com usuário inexistente: {Usuario}", loginDto.Usuario);
                    return null;
                }

                // Verifica a senha
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Senha, usuario.SenhaHash))
                {
                    _logger.LogWarning("Tentativa de login com senha incorreta para usuário: {Usuario}", loginDto.Usuario);
                    return null;
                }

                // Atualiza o último login
                usuario.UltimoLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Gera o token JWT
                var token = GerarToken(usuario);
                var expiracao = DateTime.UtcNow.AddHours(8); // Token válido por 8 horas

                _logger.LogInformation("Login realizado com sucesso para usuário: {Usuario}", usuario.NomeUsuario);

                return new AuthResponseDto
                {
                    Token = token,
                    Expiracao = expiracao,
                    Usuario = new UsuarioResponseDto
                    {
                        Id = usuario.Id,
                        NomeUsuario = usuario.NomeUsuario,
                        Email = usuario.Email,
                        NomeCompleto = usuario.NomeCompleto,
                        Perfil = usuario.Perfil,
                        DataCriacao = usuario.DataCriacao,
                        UltimoLogin = usuario.UltimoLogin
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar login para usuário: {Usuario}", loginDto.Usuario);
                return null;
            }
        }

        /// <summary>
        /// Registra um novo usuário
        /// </summary>
        public async Task<AuthResponseDto?> RegistrarAsync(RegistroDto registroDto)
        {
            try
            {
                // Verifica se o nome de usuário já existe
                if (await _context.Usuarios.AnyAsync(u => u.NomeUsuario == registroDto.NomeUsuario))
                {
                    _logger.LogWarning("Tentativa de registro com nome de usuário já existente: {NomeUsuario}", registroDto.NomeUsuario);
                    return null;
                }

                // Verifica se o email já existe
                if (await _context.Usuarios.AnyAsync(u => u.Email == registroDto.Email))
                {
                    _logger.LogWarning("Tentativa de registro com email já existente: {Email}", registroDto.Email);
                    return null;
                }

                // Cria o novo usuário
                var usuario = new Usuario
                {
                    NomeUsuario = registroDto.NomeUsuario,
                    Email = registroDto.Email,
                    NomeCompleto = registroDto.NomeCompleto,
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword(registroDto.Senha),
                    DataCriacao = DateTime.UtcNow,
                    Ativo = true,
                    Perfil = "User"
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuário registrado com sucesso: {NomeUsuario}", usuario.NomeUsuario);

                // Gera o token JWT
                var token = GerarToken(usuario);
                var expiracao = DateTime.UtcNow.AddHours(8);

                return new AuthResponseDto
                {
                    Token = token,
                    Expiracao = expiracao,
                    Usuario = new UsuarioResponseDto
                    {
                        Id = usuario.Id,
                        NomeUsuario = usuario.NomeUsuario,
                        Email = usuario.Email,
                        NomeCompleto = usuario.NomeCompleto,
                        Perfil = usuario.Perfil,
                        DataCriacao = usuario.DataCriacao,
                        UltimoLogin = null
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar usuário: {NomeUsuario}", registroDto.NomeUsuario);
                return null;
            }
        }

        /// <summary>
        /// Inicia o processo de recuperação de senha
        /// </summary>
        public async Task<bool> RecuperarSenhaAsync(string email)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == email && u.Ativo);

                if (usuario == null)
                {
                    _logger.LogWarning("Tentativa de recuperação de senha para email inexistente: {Email}", email);
                    return false;
                }

                // Gera um token de recuperação
                var token = Guid.NewGuid().ToString();
                usuario.TokenRecuperacaoSenha = token;
                usuario.TokenRecuperacaoExpiracao = DateTime.UtcNow.AddHours(1); // Token válido por 1 hora

                await _context.SaveChangesAsync();

                // Aqui você enviaria o email com o token
                // Por simplicidade, vamos apenas logar o token
                _logger.LogInformation("Token de recuperação gerado para {Email}: {Token}", email, token);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar recuperação de senha para email: {Email}", email);
                return false;
            }
        }

        /// <summary>
        /// Redefine a senha usando o token de recuperação
        /// </summary>
        public async Task<bool> RedefinirSenhaAsync(RedefinirSenhaDto redefinirSenhaDto)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => 
                        u.TokenRecuperacaoSenha == redefinirSenhaDto.Token 
                        && u.TokenRecuperacaoExpiracao > DateTime.UtcNow
                        && u.Ativo);

                if (usuario == null)
                {
                    _logger.LogWarning("Tentativa de redefinição com token inválido ou expirado: {Token}", redefinirSenhaDto.Token);
                    return false;
                }

                // Atualiza a senha
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(redefinirSenhaDto.NovaSenha);
                usuario.TokenRecuperacaoSenha = null;
                usuario.TokenRecuperacaoExpiracao = null;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Senha redefinida com sucesso para usuário: {NomeUsuario}", usuario.NomeUsuario);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao redefinir senha com token: {Token}", redefinirSenhaDto.Token);
                return false;
            }
        }

        /// <summary>
        /// Altera a senha do usuário logado
        /// </summary>
        public async Task<bool> AlterarSenhaAsync(int usuarioId, AlterarSenhaDto alterarSenhaDto)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo);

                if (usuario == null)
                {
                    _logger.LogWarning("Tentativa de alteração de senha para usuário inexistente: {UsuarioId}", usuarioId);
                    return false;
                }

                // Verifica a senha atual
                if (!BCrypt.Net.BCrypt.Verify(alterarSenhaDto.SenhaAtual, usuario.SenhaHash))
                {
                    _logger.LogWarning("Tentativa de alteração de senha com senha atual incorreta para usuário: {UsuarioId}", usuarioId);
                    return false;
                }

                // Atualiza a senha
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(alterarSenhaDto.NovaSenha);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Senha alterada com sucesso para usuário: {NomeUsuario}", usuario.NomeUsuario);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar senha para usuário: {UsuarioId}", usuarioId);
                return false;
            }
        }

        /// <summary>
        /// Obtém os dados do usuário
        /// </summary>
        public async Task<UsuarioResponseDto?> ObterUsuarioAsync(int usuarioId)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo);

                if (usuario == null)
                    return null;

                return new UsuarioResponseDto
                {
                    Id = usuario.Id,
                    NomeUsuario = usuario.NomeUsuario,
                    Email = usuario.Email,
                    NomeCompleto = usuario.NomeCompleto,
                    Perfil = usuario.Perfil,
                    DataCriacao = usuario.DataCriacao,
                    UltimoLogin = usuario.UltimoLogin
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter dados do usuário: {UsuarioId}", usuarioId);
                return null;
            }
        }

        /// <summary>
        /// Gera um token JWT para o usuário
        /// </summary>
        public string GerarToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "StefaniniCadastroPessoas2024SecretKey"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NomeUsuario),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Perfil),
                new Claim("NomeCompleto", usuario.NomeCompleto)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "StefaniniCadastroPessoas",
                audience: _configuration["Jwt:Audience"] ?? "StefaniniCadastroPessoas",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
