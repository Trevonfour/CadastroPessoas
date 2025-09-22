using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StefaniniCadastroPessoas.Data;
using StefaniniCadastroPessoas.Models;
using StefaniniCadastroPessoas.Services;
using StefaniniCadastroPessoas.DTOs;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace StefaniniCadastroPessoas.Tests
{
    public class PessoaServiceTests : IDisposable
    {
        private readonly Mock<ICpfValidationService> _mockCpfValidationService;
        private readonly Mock<ILogger<PessoaService>> _mockLogger;

        public PessoaServiceTests()
        {
            _mockCpfValidationService = new Mock<ICpfValidationService>();
            _mockLogger = new Mock<ILogger<PessoaService>>();
        }

        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new TestApplicationDbContext(options);

            return context;
        }

        public void Dispose()
        {
        }

        [Fact]
        public async Task ObterTodasAsync_DeveRetornarListaDePessoas_QuandoExistemPessoasAtivas()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            context.Pessoas.AddRange(
                new Pessoa { Id = 1, Nome = "Pessoa A", CPF = "11122233344", Ativo = true, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow },
                new Pessoa { Id = 2, Nome = "Pessoa B", CPF = "55566677788", Ativo = true, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow },
                new Pessoa { Id = 3, Nome = "Pessoa C", CPF = "99988877766", Ativo = false, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await pessoaService.ObterTodasAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Pessoas.Count());
            Assert.Equal("Pessoa A", result.Pessoas.First().Nome);
        }

        [Fact]
        public async Task ObterTodasAsync_DeveRetornarListaVazia_QuandoNaoExistemPessoasAtivas()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);
            // Nenhum dado adicionado, o banco de dados está vazio

            // Act
            var result = await pessoaService.ObterTodasAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Pessoas);
            Assert.Equal(0, result.Total);
        }

        [Fact]
        public async Task ObterTodasAsync_DeveAplicarFiltroPorNomeCPFEmailCorretamente()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            context.Pessoas.AddRange(
                new Pessoa { Id = 1, Nome = "Alice", CPF = "11122233344", Email = "alice@email.com", Ativo = true, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow },
                new Pessoa { Id = 2, Nome = "Bob", CPF = "55566677788", Email = "bob@email.com", Ativo = true, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow },
                new Pessoa { Id = 3, Nome = "Charlie", CPF = "99988877766", Email = "charlie@email.com", Ativo = true, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            // Act
            var resultNome = await pessoaService.ObterTodasAsync(filtro: "ali");
            var resultCpf = await pessoaService.ObterTodasAsync(filtro: "555");
            var resultEmail = await pessoaService.ObterTodasAsync(filtro: "charlie");

            // Assert
            Assert.Single(resultNome.Pessoas);
            Assert.Equal("Alice", resultNome.Pessoas.First().Nome);

            Assert.Single(resultCpf.Pessoas);
            Assert.Equal("Bob", resultCpf.Pessoas.First().Nome);

            Assert.Single(resultEmail.Pessoas);
            Assert.Equal("Charlie", resultEmail.Pessoas.First().Nome);
        }

        [Fact]
        public async Task ObterTodasAsync_DeveAplicarPaginacaoCorretamente()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            for (int i = 1; i <= 15; i++)
            {
                context.Pessoas.Add(new Pessoa { Id = i, Nome = $"Pessoa {i}", CPF = $"{i:D11}", Ativo = true, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow });
            }
            await context.SaveChangesAsync();

            // Act
            var pagina1 = await pessoaService.ObterTodasAsync(pagina: 1, tamanhoPagina: 5);
            var pagina2 = await pessoaService.ObterTodasAsync(pagina: 2, tamanhoPagina: 5);
            var pagina3 = await pessoaService.ObterTodasAsync(pagina: 3, tamanhoPagina: 5);

            // Assert
            Assert.Equal(5, pagina1.Pessoas.Count());
            Assert.Equal("Pessoa 1", pagina1.Pessoas.First().Nome);
            Assert.Equal(5, pagina2.Pessoas.Count());
            Assert.Equal("Pessoa 6", pagina2.Pessoas.First().Nome);
            Assert.Equal(5, pagina3.Pessoas.Count());
            Assert.Equal("Pessoa 11", pagina3.Pessoas.First().Nome);
            Assert.Equal(3, pagina1.TotalPaginas);
        }

        // Testes para ObterPorIdAsync
        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarPessoa_QuandoPessoaExisteEAtiva()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            var pessoa = new Pessoa { Id = 1, Nome = "Pessoa Teste", Ativo = true, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow };
            context.Pessoas.Add(pessoa);
            await context.SaveChangesAsync();

            // Act
            var result = await pessoaService.ObterPorIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Pessoa Teste", result.Nome);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarNull_QuandoPessoaNaoExisteOuInativa()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            var pessoaInativa = new Pessoa { Id = 1, Nome = "Pessoa Inativa", Ativo = false, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow };
            context.Pessoas.Add(pessoaInativa);
            await context.SaveChangesAsync();

            // Act
            var resultNaoExiste = await pessoaService.ObterPorIdAsync(99);
            var resultInativa = await pessoaService.ObterPorIdAsync(1);

            // Assert
            Assert.Null(resultNaoExiste);
            Assert.Null(resultInativa);
        }

        // Testes para ObterPorCpfAsync
        [Fact]
        public async Task ObterPorCpfAsync_DeveRetornarPessoa_QuandoCPFExisteEAtivo()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            var cpfLimpo = "11122233344";
            var cpfFormatado = "111.222.333-44";
            var pessoa = new Pessoa { Id = 1, Nome = "Pessoa CPF", CPF = cpfLimpo, Ativo = true, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow };
            context.Pessoas.Add(pessoa);
            await context.SaveChangesAsync();

            _mockCpfValidationService.Setup(s => s.RemoverFormatacao(cpfFormatado)).Returns(cpfLimpo);

            // Act
            var result = await pessoaService.ObterPorCpfAsync(cpfFormatado);

            // Assert
            Assert.NotNull(result);
            // CORREÇÃO: Validar contra o CPF formatado, que é o que o DTO retorna.
            Assert.Equal(cpfFormatado, result.CPF);
        }

        [Fact]
        public async Task ObterPorCpfAsync_DeveRetornarNull_QuandoCPFNaoExisteOuInativo()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            var cpfLimpo = "11122233344";
            var cpfFormatado = "111.222.333-44";
            context.Pessoas.Add(new Pessoa { Id = 1, Nome = "Pessoa Inativa", CPF = cpfLimpo, Ativo = false, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow });
            await context.SaveChangesAsync();

            _mockCpfValidationService.Setup(s => s.RemoverFormatacao(cpfFormatado)).Returns(cpfLimpo);

            // Act
            var resultNaoExiste = await pessoaService.ObterPorCpfAsync("999.999.999-99");
            var resultInativa = await pessoaService.ObterPorCpfAsync(cpfFormatado);

            // Assert
            Assert.Null(resultNaoExiste);
            Assert.Null(resultInativa);
        }

        // Testes para CriarAsync
        [Fact]
        public async Task CriarAsync_DeveCriarPessoa_QuandoCPFValidoENaoExistente()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            var criarPessoaDto = new CriarPessoaDto
            {
                Nome = "Nova Pessoa",
                CPF = "123.456.789-00",
                Email = "nova@example.com",
                DataNascimento = new DateTime(1990, 1, 1),
                Sexo = "M",
                Naturalidade = "Cidade",
                Nacionalidade = "Pais"
            };

            _mockCpfValidationService.Setup(s => s.ValidarCpf(criarPessoaDto.CPF)).Returns(true);
            _mockCpfValidationService.Setup(s => s.RemoverFormatacao(criarPessoaDto.CPF)).Returns("12345678900");

            // Act
            var result = await pessoaService.CriarAsync(criarPessoaDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Nova Pessoa", result.Nome);
            Assert.Equal(1, context.Pessoas.Count()); // Verifica se a pessoa foi adicionada ao contexto
        }

        [Fact]
        public async Task CriarAsync_NaoDeveCriarPessoa_QuandoCPFInvalido()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            var criarPessoaDto = new CriarPessoaDto
            {
                Nome = "Pessoa Invalida",
                CPF = "123",
                Email = "invalida@example.com",
                DataNascimento = new DateTime(1990, 1, 1),
                Sexo = "F",
                Naturalidade = "Cidade",
                Nacionalidade = "Pais"
            };

            _mockCpfValidationService.Setup(s => s.ValidarCpf(criarPessoaDto.CPF)).Returns(false);

            // Act
            var result = await pessoaService.CriarAsync(criarPessoaDto);

            // Assert
            Assert.Null(result);
            Assert.Equal(0, context.Pessoas.Count());
        }

        [Fact]
        public async Task CriarAsync_NaoDeveCriarPessoa_QuandoCPFJaExiste()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            var cpfLimpo = "11122233344";
            var cpfFormatado = "111.222.333-44";
            context.Pessoas.Add(new Pessoa { Id = 1, Nome = "Pessoa Existente", CPF = cpfLimpo, Ativo = true, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var criarPessoaDto = new CriarPessoaDto
            {
                Nome = "Pessoa Duplicada",
                CPF = cpfFormatado,
                Email = "duplicada@example.com",
                DataNascimento = new DateTime(1985, 5, 10),
                Sexo = "M",
                Naturalidade = "Outra Cidade",
                Nacionalidade = "Outro Pais"
            };

            _mockCpfValidationService.Setup(s => s.ValidarCpf(criarPessoaDto.CPF)).Returns(true);
            _mockCpfValidationService.Setup(s => s.RemoverFormatacao(criarPessoaDto.CPF)).Returns(cpfLimpo);

            // Act
            var result = await pessoaService.CriarAsync(criarPessoaDto);

            // Assert
            Assert.Null(result);
            Assert.Equal(1, context.Pessoas.Count()); // Apenas a pessoa original deve estar no banco
        }

        // Testes para AtualizarAsync
        [Fact]
        public async Task AtualizarAsync_DeveAtualizarPessoa_QuandoPessoaExiste()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            var pessoaExistente = new Pessoa { Id = 1, Nome = "Pessoa Original", Ativo = true, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow };
            context.Pessoas.Add(pessoaExistente);
            await context.SaveChangesAsync();

            var atualizarPessoaDto = new AtualizarPessoaDto { Nome = "Pessoa Atualizada", Email = "atualizado@email.com" };

            // Act
            var result = await pessoaService.AtualizarAsync(1, atualizarPessoaDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Pessoa Atualizada", result.Nome);
            Assert.Equal("atualizado@email.com", result.Email);
            var pessoaNoBanco = await context.Pessoas.FindAsync(1);
            // CORREÇÃO: Adicionado '!' para resolver aviso CS8602
            Assert.Equal("Pessoa Atualizada", pessoaNoBanco!.Nome);
        }

        [Fact]
        public async Task AtualizarAsync_NaoDeveAtualizarPessoa_QuandoPessoaNaoExiste()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            var atualizarPessoaDto = new AtualizarPessoaDto { Nome = "Pessoa Atualizada" };

            // Act
            var result = await pessoaService.AtualizarAsync(99, atualizarPessoaDto);

            // Assert
            Assert.Null(result);
            Assert.Equal(0, context.Pessoas.Count());
        }

        // Testes para RemoverAsync
        [Fact]
        public async Task RemoverAsync_DeveMarcarPessoaComoInativa_QuandoPessoaExiste()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            var pessoaExistente = new Pessoa { Id = 1, Nome = "Pessoa Ativa", Ativo = true, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow };
            context.Pessoas.Add(pessoaExistente);
            await context.SaveChangesAsync();

            // Act
            var result = await pessoaService.RemoverAsync(1);

            // Assert
            Assert.True(result);
            var pessoaNoBanco = await context.Pessoas.FindAsync(1);
            // CORREÇÃO: Adicionado '!' para resolver aviso CS8602
            Assert.False(pessoaNoBanco!.Ativo);
        }

        [Fact]
        public async Task RemoverAsync_NaoDeveRemoverPessoa_QuandoPessoaNaoExiste()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);
            // Nenhum dado adicionado

            // Act
            var result = await pessoaService.RemoverAsync(99);

            // Assert
            Assert.False(result);
            Assert.Equal(0, context.Pessoas.Count());
        }

        // Testes para ExisteCpfAsync
        [Fact]
        public async Task ExisteCpfAsync_DeveRetornarTrue_QuandoCPFExisteEAtivo()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            var cpfLimpo = "11122233344";
            var cpfFormatado = "111.222.333-44";
            context.Pessoas.Add(new Pessoa { Id = 1, CPF = cpfLimpo, Ativo = true, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow });
            await context.SaveChangesAsync();

            _mockCpfValidationService.Setup(s => s.RemoverFormatacao(It.IsAny<string>())).Returns(cpfLimpo);

            // Act
            var result = await pessoaService.ExisteCpfAsync(cpfFormatado);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExisteCpfAsync_DeveRetornarFalse_QuandoCPFNaoExisteOuInativo()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            var cpfLimpo = "11122233344";
            var cpfFormatado = "111.222.333-44";
            context.Pessoas.Add(new Pessoa { Id = 1, CPF = cpfLimpo, Ativo = false, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow });
            await context.SaveChangesAsync();

            _mockCpfValidationService.Setup(s => s.RemoverFormatacao(It.IsAny<string>())).Returns(cpfLimpo);

            // Act
            var resultNaoExiste = await pessoaService.ExisteCpfAsync("999.999.999-99");
            var resultInativa = await pessoaService.ExisteCpfAsync(cpfFormatado);

            // Assert
            Assert.False(resultNaoExiste);
            Assert.False(resultInativa);
        }

        // CORREÇÃO: Lógica do teste ajustada para validar o cenário correto.
        [Fact]
        public async Task ExisteCpfAsync_DeveRetornarFalse_QuandoCPFExisteMasPertenceAoIdExcluido()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var pessoaService = new PessoaService(context, _mockCpfValidationService.Object, _mockLogger.Object);

            var cpfLimpo = "11122233344";
            var cpfFormatado = "111.222.333-44";
            context.Pessoas.Add(new Pessoa { Id = 1, CPF = cpfLimpo, Ativo = true, DataCadastro = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow });
            await context.SaveChangesAsync();

            _mockCpfValidationService.Setup(s => s.RemoverFormatacao(It.IsAny<string>())).Returns(cpfLimpo);

            // Act: Verifica se o CPF existe, IGNORANDO o próprio dono do CPF (ID 1)
            var result = await pessoaService.ExisteCpfAsync(cpfFormatado, idExcluir: 1);

            // Assert: Deve retornar False, pois não há OUTRA pessoa com este CPF.
            Assert.False(result);
        }
    }
}
