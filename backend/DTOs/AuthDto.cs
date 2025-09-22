using System.ComponentModel.DataAnnotations;

namespace StefaniniCadastroPessoas.DTOs
{
    /// <summary>
    /// DTO para login
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Nome de usuário ou email
        /// </summary>
        [Required(ErrorMessage = "Nome de usuário ou email é obrigatório")]
        public string Usuario { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário
        /// </summary>
        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Senha { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para registro de usuário
    /// </summary>
    public class RegistroDto
    {
        /// <summary>
        /// Nome de usuário
        /// </summary>
        [Required(ErrorMessage = "Nome de usuário é obrigatório")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Nome de usuário deve ter entre 3 e 50 caracteres")]
        public string NomeUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuário
        /// </summary>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter formato válido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        [Required(ErrorMessage = "Nome completo é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome completo deve ter no máximo 200 caracteres")]
        public string NomeCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Senha do usuário
        /// </summary>
        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
        public string Senha { get; set; } = string.Empty;

        /// <summary>
        /// Confirmação da senha
        /// </summary>
        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [Compare("Senha", ErrorMessage = "Senha e confirmação devem ser iguais")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para resposta de autenticação
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// Token JWT
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Data de expiração do token
        /// </summary>
        public DateTime Expiracao { get; set; }

        /// <summary>
        /// Dados do usuário
        /// </summary>
        public UsuarioResponseDto Usuario { get; set; } = new();
    }

    /// <summary>
    /// DTO para resposta de usuário
    /// </summary>
    public class UsuarioResponseDto
    {
        /// <summary>
        /// ID do usuário
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome de usuário
        /// </summary>
        public string NomeUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuário
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        public string NomeCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Perfil do usuário
        /// </summary>
        public string Perfil { get; set; } = string.Empty;

        /// <summary>
        /// Data de criação da conta
        /// </summary>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Data do último login
        /// </summary>
        public DateTime? UltimoLogin { get; set; }
    }

    /// <summary>
    /// DTO para recuperação de senha
    /// </summary>
    public class RecuperarSenhaDto
    {
        /// <summary>
        /// Email do usuário
        /// </summary>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter formato válido")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para redefinir senha
    /// </summary>
    public class RedefinirSenhaDto
    {
        /// <summary>
        /// Token de recuperação
        /// </summary>
        [Required(ErrorMessage = "Token é obrigatório")]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Nova senha
        /// </summary>
        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
        public string NovaSenha { get; set; } = string.Empty;

        /// <summary>
        /// Confirmação da nova senha
        /// </summary>
        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [Compare("NovaSenha", ErrorMessage = "Senha e confirmação devem ser iguais")]
        public string ConfirmarNovaSenha { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para alterar senha
    /// </summary>
    public class AlterarSenhaDto
    {
        /// <summary>
        /// Senha atual
        /// </summary>
        [Required(ErrorMessage = "Senha atual é obrigatória")]
        public string SenhaAtual { get; set; } = string.Empty;

        /// <summary>
        /// Nova senha
        /// </summary>
        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
        public string NovaSenha { get; set; } = string.Empty;

        /// <summary>
        /// Confirmação da nova senha
        /// </summary>
        [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
        [Compare("NovaSenha", ErrorMessage = "Senha e confirmação devem ser iguais")]
        public string ConfirmarNovaSenha { get; set; } = string.Empty;
    }
}
