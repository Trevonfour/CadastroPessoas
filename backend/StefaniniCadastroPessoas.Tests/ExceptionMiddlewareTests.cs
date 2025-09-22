using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using StefaniniCadastroPessoas.Middleware;

namespace StefaniniCadastroPessoas.Tests
{
    /// <summary>
    /// Testes de integração para o ExceptionMiddleware.
    /// </summary>
    public class ExceptionMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ExceptionMiddlewareTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Testa o caminho feliz: se nenhuma exceção for lançada, o middleware
        /// deve simplesmente passar a requisição adiante.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_QuandoNenhumaExcecaoOcorre_DeveRetornarOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            // Usamos um endpoint que sabemos que funciona, como o /health
            var response = await client.GetAsync("/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Testa se uma exceção genérica é capturada e formatada como 500 Internal Server Error.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_QuandoExcecaoGenericaOcorre_DeveRetornar500()
        {
            // Arrange
            // Criamos uma fábrica de aplicação customizada que inclui um endpoint para lançar um erro.
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.Configure(app =>
                {
                    app.UseGlobalExceptionHandler(); // Importante usar nosso middleware
                    app.Run(async context =>
                    {
                        throw new Exception("Erro de teste genérico");
                    });
                });
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/qualquer-rota");

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            Assert.NotNull(errorResponse);
            Assert.Equal("Erro interno do servidor", errorResponse.Message);
        }

        /// <summary>
        /// Testa se uma exceção específica (ArgumentException) é mapeada para 400 Bad Request.
        /// </summary>
        [Fact]
        public async Task InvokeAsync_QuandoArgumentExceptionOcorre_DeveRetornar400()
        {
            // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.Configure(app =>
                {
                    app.UseGlobalExceptionHandler();
                    app.Run(async context =>
                    {
                        throw new ArgumentException("Parâmetro de teste inválido");
                    });
                });
            }).CreateClient();
            
            // Act
            var response = await client.GetAsync("/rota-com-erro-de-argumento");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(errorResponse);
            Assert.Equal("Parâmetros inválidos", errorResponse.Message);
            Assert.Equal("Parâmetro de teste inválido", errorResponse.Details);
        }

        /// <summary>
        /// Testa se, em ambiente de Desenvolvimento, o StackTrace é incluído na resposta.
        /// </summary>
        [Fact]
        public async Task HandleExceptionAsync_EmDesenvolvimento_DeveIncluirStackTrace()
        {
             // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
            {
                // Força o ambiente para "Development"
                builder.UseEnvironment("Development");
                builder.Configure(app =>
                {
                    app.UseGlobalExceptionHandler();
                    app.Run(async context =>
                    {
                        throw new Exception("Erro de teste");
                    });
                });
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/erro-dev");
            var content = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            Assert.NotNull(errorResponse);
            Assert.False(string.IsNullOrWhiteSpace(errorResponse.StackTrace));
        }

        /// <summary>
        /// Testa se, em ambiente de Produção, o StackTrace NÃO é incluído na resposta.
        /// </summary>
        [Fact]
        public async Task HandleExceptionAsync_EmProducao_NaoDeveIncluirStackTrace()
        {
             // Arrange
            var client = _factory.WithWebHostBuilder(builder =>
            {
                // Força o ambiente para "Production"
                builder.UseEnvironment("Production");
                builder.Configure(app =>
                {
                    app.UseGlobalExceptionHandler();
                    app.Run(async context =>
                    {
                        throw new Exception("Erro de teste");
                    });
                });
            }).CreateClient();

            // Act
            var response = await client.GetAsync("/erro-prod");
            var content = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            Assert.NotNull(errorResponse);
            Assert.Null(errorResponse.StackTrace);
            Assert.Equal("Ocorreu um erro inesperado. Tente novamente mais tarde.", errorResponse.Details);
        }
    }
}
