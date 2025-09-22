using System.ComponentModel.DataAnnotations;

namespace StefaniniCadastroPessoas.Models
{
    /// <summary>
    /// Modelo que representa um usuário do sistema
    /// </summary>
    public class Usuario
    {
        /// <summary>
        /// Identificador único do usuário
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome de usuário (login)
        /// </summary>
        [Required(ErrorMessage = "Nome de usuário é obrigatório")]
        [StringLength(50, ErrorMessage = "Nome de usuário deve ter no máximo 50 caracteres")]
        public string NomeUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Email do usuário
        /// </summary>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter formato válido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Hash da senha do usuário
        /// </summary>
        [Required]
        public string SenhaHash { get; set; } = string.Empty;

        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        [Required(ErrorMessage = "Nome completo é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome completo deve ter no máximo 200 caracteres")]
        public string NomeCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Data de criação da conta
        /// </summary>
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data do último login
        /// </summary>
        public DateTime? UltimoLogin { get; set; }

        /// <summary>
        /// Indica se a conta está ativa
        /// </summary>
        public bool Ativo { get; set; } = true;

        /// <summary>
        /// Token para recuperação de senha
        /// </summary>
        public string? TokenRecuperacaoSenha { get; set; }

        /// <summary>
        /// Data de expiração do token de recuperação
        /// </summary>
        public DateTime? TokenRecuperacaoExpiracao { get; set; }

        /// <summary>
        /// Perfil do usuário (Admin, User, etc.)
        /// </summary>
        [StringLength(20)]
        public string Perfil { get; set; } = "User";
    }
}
