using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Application.DTOs;

namespace API.Tests
{
    public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public ProductsControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<string> GetTokenAsync(string username, string password)
        {
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
            {
                Username = username,
                Password = password
            });

            response.EnsureSuccessStatusCode();

            var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
            Assert.NotNull(authResult);
            return authResult.Token;
        }

        [Fact]
        public async Task GetProducts_AnonymousAccess_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/products");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<object>();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateProduct_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            var newProduct = new CreateProductDto { ProductName = "Unauthorized Product" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/products", newProduct);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateProduct_WithToken_ReturnsCreatedAndSaves()
        {
            // Arrange
            var token = await GetTokenAsync("user", "UserPassword123!");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var newProduct = new CreateProductDto { ProductName = "Tablet Z" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/products", newProduct);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var productResult = await response.Content.ReadFromJsonAsync<ProductDto>();
            Assert.NotNull(productResult);
            Assert.True(productResult.Id > 0);
            Assert.Equal("Tablet Z", productResult.ProductName);
            Assert.Equal("user", productResult.CreatedBy);
        }

        [Fact]
        public async Task DeleteProduct_AsNormalUser_ReturnsForbidden()
        {
            // Arrange
            var token = await GetTokenAsync("user", "UserPassword123!");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act - Delete product with ID 1
            var response = await _client.DeleteAsync("/api/v1/products/1");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_AsAdminUser_ReturnsNoContent()
        {
            // Arrange
            var token = await GetTokenAsync("admin", "AdminPassword123!");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act - Delete product with ID 1 (should exist as it's seeded on startup)
            var response = await _client.DeleteAsync("/api/v1/products/1");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
