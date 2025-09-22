using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StefaniniCadastroPessoas.Models
{
    /// <summary>
    /// Modelo que representa uma pessoa no sistema
    /// </summary>
    public class Pessoa
    {
        /// <summary>
        /// Identificador único da pessoa
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome completo da pessoa (obrigatório)
        /// </summary>
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Sexo da pessoa (opcional)
        /// M = Masculino, F = Feminino, O = Outro
        /// </summary>
        [StringLength(1)]
        public string? Sexo { get; set; }

        /// <summary>
        /// Email da pessoa (opcional, mas deve ser válido se preenchido)
        /// </summary>
        [EmailAddress(ErrorMessage = "Email deve ter formato válido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string? Email { get; set; }

        /// <summary>
        /// Data de nascimento da pessoa (obrigatória)
        /// </summary>
        [Required(ErrorMessage = "Data de nascimento é obrigatória")]
        [DataType(DataType.Date)]
        public DateTime DataNascimento { get; set; }

        /// <summary>
        /// Naturalidade da pessoa (opcional)
        /// </summary>
        [StringLength(100, ErrorMessage = "Naturalidade deve ter no máximo 100 caracteres")]
        public string? Naturalidade { get; set; }

        /// <summary>
        /// Nacionalidade da pessoa (opcional)
        /// </summary>
        [StringLength(100, ErrorMessage = "Nacionalidade deve ter no máximo 100 caracteres")]
        public string? Nacionalidade { get; set; }

        /// <summary>
        /// CPF da pessoa (obrigatório e deve ser válido)
        /// </summary>
        [Required(ErrorMessage = "CPF é obrigatório")]
        [StringLength(14, ErrorMessage = "CPF deve ter formato XXX.XXX.XXX-XX")]
        public string CPF { get; set; } = string.Empty;

        /// <summary>
        /// Data de cadastro da pessoa
        /// </summary>
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data da última atualização dos dados
        /// </summary>
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica se o registro está ativo
        /// </summary>
        public bool Ativo { get; set; } = true;

        /// <summary>
        /// Calcula a idade da pessoa baseada na data de nascimento
        /// </summary>
        [NotMapped]
        public int Idade
        {
            get
            {
                var hoje = DateTime.Today;
                var idade = hoje.Year - DataNascimento.Year;
                if (DataNascimento.Date > hoje.AddYears(-idade))
                    idade--;
                return idade;
            }
        }

        /// <summary>
        /// Retorna o CPF formatado
        /// </summary>
        [NotMapped]
        public string CPFFormatado
        {
            get
            {
                if (string.IsNullOrEmpty(CPF) || CPF.Length != 11)
                    return CPF;
                
                return $"{CPF.Substring(0, 3)}.{CPF.Substring(3, 3)}.{CPF.Substring(6, 3)}-{CPF.Substring(9, 2)}";
            }
        }
    }
}
