using System.ComponentModel.DataAnnotations;

namespace StefaniniCadastroPessoas.DTOs
{
    /// <summary>
    /// DTO para criação de pessoa
    /// </summary>
    public class CriarPessoaDto
    {
        /// <summary>
        /// Nome completo da pessoa
        /// </summary>
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Sexo da pessoa (M/F/O)
        /// </summary>
        [RegularExpression("^[MFO]$", ErrorMessage = "Sexo deve ser M, F ou O")]
        public string? Sexo { get; set; }

        /// <summary>
        /// Email da pessoa
        /// </summary>
        [EmailAddress(ErrorMessage = "Email deve ter formato válido")]
        public string? Email { get; set; }

        /// <summary>
        /// Data de nascimento
        /// </summary>
        [Required(ErrorMessage = "Data de nascimento é obrigatória")]
        public DateTime DataNascimento { get; set; }

        /// <summary>
        /// Naturalidade da pessoa
        /// </summary>
        public string? Naturalidade { get; set; }

        /// <summary>
        /// Nacionalidade da pessoa
        /// </summary>
        public string? Nacionalidade { get; set; }

        /// <summary>
        /// CPF da pessoa (apenas números)
        /// </summary>
        [Required(ErrorMessage = "CPF é obrigatório")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter exatamente 11 dígitos")]
        public string CPF { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para atualização de pessoa
    /// </summary>
    public class AtualizarPessoaDto
    {
        /// <summary>
        /// Nome completo da pessoa
        /// </summary>
        [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres")]
        public string? Nome { get; set; }

        /// <summary>
        /// Sexo da pessoa (M/F/O)
        /// </summary>
        [RegularExpression("^[MFO]$", ErrorMessage = "Sexo deve ser M, F ou O")]
        public string? Sexo { get; set; }

        /// <summary>
        /// Email da pessoa
        /// </summary>
        [EmailAddress(ErrorMessage = "Email deve ter formato válido")]
        public string? Email { get; set; }

        /// <summary>
        /// Data de nascimento
        /// </summary>
        public DateTime? DataNascimento { get; set; }

        /// <summary>
        /// Naturalidade da pessoa
        /// </summary>
        public string? Naturalidade { get; set; }

        /// <summary>
        /// Nacionalidade da pessoa
        /// </summary>
        public string? Nacionalidade { get; set; }
    }

    /// <summary>
    /// DTO para resposta de pessoa
    /// </summary>
    public class PessoaResponseDto
    {
        /// <summary>
        /// ID da pessoa
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome completo
        /// </summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Sexo
        /// </summary>
        public string? Sexo { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Data de nascimento
        /// </summary>
        public DateTime DataNascimento { get; set; }

        /// <summary>
        /// Idade calculada
        /// </summary>
        public int Idade { get; set; }

        /// <summary>
        /// Naturalidade
        /// </summary>
        public string? Naturalidade { get; set; }

        /// <summary>
        /// Nacionalidade
        /// </summary>
        public string? Nacionalidade { get; set; }

        /// <summary>
        /// CPF formatado
        /// </summary>
        public string CPF { get; set; } = string.Empty;

        /// <summary>
        /// Data de cadastro
        /// </summary>
        public DateTime DataCadastro { get; set; }

        /// <summary>
        /// Data de atualização
        /// </summary>
        public DateTime DataAtualizacao { get; set; }
    }

    /// <summary>
    /// DTO para listagem paginada de pessoas
    /// </summary>
    public class PessoaListResponseDto
    {
        /// <summary>
        /// Lista de pessoas
        /// </summary>
        public List<PessoaResponseDto> Pessoas { get; set; } = new();

        /// <summary>
        /// Total de registros
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Página atual
        /// </summary>
        public int Pagina { get; set; }

        /// <summary>
        /// Tamanho da página
        /// </summary>
        public int TamanhoPagina { get; set; }

        /// <summary>
        /// Total de páginas
        /// </summary>
        public int TotalPaginas { get; set; }
    }
}
