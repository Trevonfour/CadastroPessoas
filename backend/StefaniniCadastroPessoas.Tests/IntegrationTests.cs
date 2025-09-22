using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using StefaniniCadastroPessoas.Data; // Adicionado para a classe Program

namespace StefaniniCadastroPessoas.Tests
{
    /// <summary>
    /// Testes de integração para a API, validando endpoints e o pipeline de configuração.
    /// A classe precisa ser public para ser descoberta pelo test runner do xUnit.
    /// </summary>
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Testa o endpoint público /health para garantir que a API está rodando e respondendo.
        /// </summary>
        [Fact]
        public async Task HealthCheck_Endpoint_DeveRetornarOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode(); 
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

            var responseString = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseString);
            var status = jsonDoc.RootElement.GetProperty("status").GetString();
            
            Assert.Equal("Healthy", status);
        }

        /// <summary>
        /// Testa um endpoint protegido sem autenticação para garantir que o middleware de autorização está funcionando.
        /// </summary>
        [Fact]
        public async Task EndpointProtegido_SemToken_DeveRetornarUnauthorized()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            // CORREÇÃO: A URL foi ajustada para a rota correta do endpoint.
            var response = await client.GetAsync("/api/pessoas");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}

