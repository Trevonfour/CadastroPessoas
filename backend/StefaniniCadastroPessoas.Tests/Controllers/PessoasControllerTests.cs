using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StefaniniCadastroPessoas.Controllers;
using StefaniniCadastroPessoas.Services;
using StefaniniCadastroPessoas.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace StefaniniCadastroPessoas.Tests
{
    /// <summary>
    /// Testes de unidade para a classe PessoasController.
    /// </summary>
    public class PessoasControllerTests
    {
        private readonly Mock<IPessoaService> _mockPessoaService;
        private readonly Mock<ILogger<PessoasController>> _mockLogger;
        private readonly PessoasController _controller;

        public PessoasControllerTests()
        {
            _mockPessoaService = new Mock<IPessoaService>();
            _mockLogger = new Mock<ILogger<PessoasController>>();
            _controller = new PessoasController(_mockPessoaService.Object, _mockLogger.Object);
        }

        // --- Testes para ObterTodas ---

        [Fact]
        public async Task ObterTodas_ComParametrosValidos_DeveRetornarOkComListaDePessoas()
        {
            // Arrange
            var listaPessoas = new PessoaListResponseDto { Pessoas = new List<PessoaResponseDto> { new PessoaResponseDto() } };
            _mockPessoaService.Setup(s => s.ObterTodasAsync(1, 10, null)).ReturnsAsync(listaPessoas);

            // Act
            var result = await _controller.ObterTodas();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(listaPessoas, okResult.Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ObterTodas_ComPaginaInvalida_DeveRetornarBadRequest(int pagina)
        {
            // Act
            var result = await _controller.ObterTodas(pagina: pagina);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public async Task ObterTodas_ComTamanhoPaginaInvalido_DeveRetornarBadRequest(int tamanhoPagina)
        {
            // Act
            var result = await _controller.ObterTodas(tamanhoPagina: tamanhoPagina);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        // --- Testes para ObterPorId ---

        [Fact]
        public async Task ObterPorId_QuandoPessoaExiste_DeveRetornarOkComPessoa()
        {
            // Arrange
            var pessoaDto = new PessoaResponseDto { Id = 1, Nome = "João da Silva" };
            _mockPessoaService.Setup(s => s.ObterPorIdAsync(1)).ReturnsAsync(pessoaDto);

            // Act
            var result = await _controller.ObterPorId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(pessoaDto, okResult.Value);
        }

        [Fact]
        public async Task ObterPorId_QuandoPessoaNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            _mockPessoaService.Setup(s => s.ObterPorIdAsync(It.IsAny<int>())).ReturnsAsync((PessoaResponseDto?)null);

            // Act
            var result = await _controller.ObterPorId(99);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        // --- Testes para Criar ---

        [Fact]
        public async Task Criar_ComDadosValidos_DeveRetornarCreatedAtAction()
        {
            // Arrange
            var criarDto = new CriarPessoaDto { Nome = "Maria", CPF = "123.456.789-00" };
            var pessoaCriadaDto = new PessoaResponseDto { Id = 1, Nome = "Maria", CPF = "123.456.789-00" };
            _mockPessoaService.Setup(s => s.CriarAsync(criarDto)).ReturnsAsync(pessoaCriadaDto);

            // Act
            var result = await _controller.Criar(criarDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdAtActionResult.StatusCode);
            Assert.Equal(nameof(PessoasController.ObterPorId), createdAtActionResult.ActionName);
            Assert.Equal(pessoaCriadaDto, createdAtActionResult.Value);
        }

        [Fact]
        public async Task Criar_ComCpfJaExistente_DeveRetornarBadRequest()
        {
            // Arrange
            var criarDto = new CriarPessoaDto { Nome = "Maria", CPF = "123.456.789-00" };
            _mockPessoaService.Setup(s => s.CriarAsync(criarDto)).ReturnsAsync((PessoaResponseDto?)null);

            // Act
            var result = await _controller.Criar(criarDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task Criar_ComModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("CPF", "CPF é obrigatório");
            var criarDto = new CriarPessoaDto();

            // Act
            var result = await _controller.Criar(criarDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        // --- Testes para Atualizar ---

        [Fact]
        public async Task Atualizar_QuandoPessoaExiste_DeveRetornarOkComPessoaAtualizada()
        {
            // Arrange
            var atualizarDto = new AtualizarPessoaDto { Nome = "João Atualizado" };
            var pessoaAtualizadaDto = new PessoaResponseDto { Id = 1, Nome = "João Atualizado" };
            _mockPessoaService.Setup(s => s.AtualizarAsync(1, atualizarDto)).ReturnsAsync(pessoaAtualizadaDto);

            // Act
            var result = await _controller.Atualizar(1, atualizarDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(pessoaAtualizadaDto, okResult.Value);
        }

        [Fact]
        public async Task Atualizar_QuandoPessoaNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            var atualizarDto = new AtualizarPessoaDto { Nome = "Qualquer Nome" };
            _mockPessoaService.Setup(s => s.AtualizarAsync(It.IsAny<int>(), It.IsAny<AtualizarPessoaDto>()))
                .ReturnsAsync((PessoaResponseDto?)null);

            // Act
            var result = await _controller.Atualizar(99, atualizarDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        // --- Testes para Remover ---

        [Fact]
        public async Task Remover_QuandoPessoaExiste_DeveRetornarNoContent()
        {
            // Arrange
            _mockPessoaService.Setup(s => s.RemoverAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.Remover(1);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task Remover_QuandoPessoaNaoExiste_DeveRetornarNotFound()
        {
            // Arrange
            _mockPessoaService.Setup(s => s.RemoverAsync(99)).ReturnsAsync(false);

            // Act
            var result = await _controller.Remover(99);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
        
        // --- Teste para Exceção Genérica ---

        [Fact]
        public async Task ObterTodas_QuandoServicoLancaExcecao_DeveRetornarStatusCode500()
        {
            // Arrange
            _mockPessoaService.Setup(s => s.ObterTodasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Erro de banco de dados simulado"));

            // Act
            var result = await _controller.ObterTodas();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
        }
    }
}
