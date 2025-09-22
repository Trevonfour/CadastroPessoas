using Xunit;
using StefaniniCadastroPessoas.Services;

namespace StefaniniCadastroPessoas.Tests
{
    /// <summary>
    /// Testes de unidade para a classe CpfValidationService.
    /// </summary>
    public class CpfValidationServiceTests
    {
        private readonly CpfValidationService _cpfService;

        public CpfValidationServiceTests()
        {
            _cpfService = new CpfValidationService();
        }

        // --- Testes para o método ValidarCpf ---

        [Theory]
        [InlineData("22442001403")]      // CPF Válido (verificado)
        [InlineData("224.420.014-03")]  // CPF Válido (verificado e formatado)
        public void ValidarCpf_ComCpfValido_DeveRetornarTrue(string cpf)
        {
            // Act
            var resultado = _cpfService.ValidarCpf(cpf);

            // Assert
            Assert.True(resultado, $"CPF '{cpf}' deveria ser válido, mas foi considerado inválido.");
        }

        [Theory]
        [InlineData("11111111111")]      // Dígitos repetidos
        [InlineData("12345678901")]      // Dígitos verificadores incorretos
        [InlineData("123.456.789-01")]  // Dígitos verificadores incorretos (formatado)
        [InlineData("12345")]           // Comprimento incorreto
        [InlineData("abcdefghijk")]     // Contém letras
        [InlineData("")]                // Vazio
        [InlineData(null)]              // Nulo
        public void ValidarCpf_ComCpfInvalido_DeveRetornarFalse(string? cpf)
        {
            // Act
            var resultado = _cpfService.ValidarCpf(cpf);

            // Assert
            Assert.False(resultado, $"CPF '{cpf}' deveria ser inválido, mas foi considerado válido.");
        }

        // --- Testes para o método RemoverFormatacao ---

        [Fact]
        public void RemoverFormatacao_ComCpfFormatado_DeveRetornarApenasNumeros()
        {
            // Arrange
            var cpfFormatado = "123.456.789-00";
            var cpfEsperado = "12345678900";

            // Act
            var resultado = _cpfService.RemoverFormatacao(cpfFormatado);

            // Assert
            Assert.Equal(cpfEsperado, resultado);
        }
        
        [Fact]
        public void RemoverFormatacao_ComCpfComEspacos_DeveRetornarApenasNumeros()
        {
            // Arrange
            var cpfComEspacos = " 123 456 789 00 ";
            var cpfEsperado = "12345678900";

            // Act
            var resultado = _cpfService.RemoverFormatacao(cpfComEspacos);

            // Assert
            Assert.Equal(cpfEsperado, resultado);
        }

        [Fact]
        public void RemoverFormatacao_ComStringNulaOuVazia_DeveRetornarStringVazia()
        {
            // Act
            var resultadoNulo = _cpfService.RemoverFormatacao(null);
            var resultadoVazio = _cpfService.RemoverFormatacao("");

            // Assert
            Assert.Empty(resultadoNulo);
            Assert.Empty(resultadoVazio);
        }

        // --- Testes para o método FormatarCpf ---

        [Fact]
        public void FormatarCpf_ComCpfApenasNumeros_DeveRetornarCpfFormatado()
        {
            // Arrange
            var cpfApenasNumeros = "12345678900";
            var cpfEsperado = "123.456.789-00";

            // Act
            var resultado = _cpfService.FormatarCpf(cpfApenasNumeros);

            // Assert
            Assert.Equal(cpfEsperado, resultado);
        }

        [Fact]
        public void FormatarCpf_ComCpfJaFormatado_DeveRetornarCpfFormatadoCorretamente()
        {
            // Arrange
            var cpfJaFormatado = "987.654.321-00";
            var cpfEsperado = "987.654.321-00";

            // Act
            var resultado = _cpfService.FormatarCpf(cpfJaFormatado);

            // Assert
            Assert.Equal(cpfEsperado, resultado);
        }

        [Fact]
        public void FormatarCpf_ComComprimentoInvalido_DeveRetornarStringOriginalSemFormatacao()
        {
            // Arrange
            var cpfInvalido = "12345";
            
            // Act
            var resultado = _cpfService.FormatarCpf(cpfInvalido);

            // Assert
            Assert.Equal(cpfInvalido, resultado);
        }
        
        [Fact]
        public void FormatarCpf_ComStringNulaOuVazia_DeveRetornarStringVazia()
        {
            // Act
            var resultadoNulo = _cpfService.FormatarCpf(null);
            var resultadoVazio = _cpfService.FormatarCpf("");

            // Assert
            Assert.Empty(resultadoNulo);
            Assert.Empty(resultadoVazio);
        }
    }
}

