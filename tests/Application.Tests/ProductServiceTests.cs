using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;

namespace Application.Tests
{
    public class ProductServiceTests
    {
        private ApplicationDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateDbContext(dbName);
            var product = new Product { Id = 1, ProductName = "Test Product", CreatedBy = "test_user", CreatedOn = DateTime.UtcNow };
            context.Products.Add(product);
            context.Items.Add(new Item { Id = 1, ProductId = 1, Quantity = 10 });
            await context.SaveChangesAsync();

            using var uow = new UnitOfWork(context);
            var service = new ProductService(uow);

            // Act
            var result = await service.GetProductByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Product", result.ProductName);
            Assert.Single(result.Items);
            Assert.Equal(10, result.Items[0].Quantity);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithInvalidId_ThrowsNotFoundException()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateDbContext(dbName);
            using var uow = new UnitOfWork(context);
            var service = new ProductService(uow);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => service.GetProductByIdAsync(999));
        }

        [Fact]
        public async Task CreateProductAsync_ValidPayload_ReturnsCreatedProduct()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateDbContext(dbName);
            using var uow = new UnitOfWork(context);
            var service = new ProductService(uow);
            var createDto = new CreateProductDto { ProductName = "New Product" };

            // Act
            var result = await service.CreateProductAsync(createDto, "user1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("New Product", result.ProductName);
            Assert.Equal("user1", result.CreatedBy);

            var dbProduct = await context.Products.FindAsync(result.Id);
            Assert.NotNull(dbProduct);
            Assert.Equal("New Product", dbProduct.ProductName);
        }

        [Fact]
        public async Task GetProductsAsync_WithPagination_ReturnsPaginatedProducts()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateDbContext(dbName);
            for (int i = 1; i <= 15; i++)
            {
                context.Products.Add(new Product { Id = i, ProductName = $"Product {i}", CreatedBy = "admin", CreatedOn = DateTime.UtcNow });
            }
            await context.SaveChangesAsync();

            using var uow = new UnitOfWork(context);
            var service = new ProductService(uow);

            // Act
            var (products, totalCount) = await service.GetProductsAsync(pageNumber: 2, pageSize: 5);

            // Assert
            Assert.Equal(15, totalCount);
            var list = products.ToList();
            Assert.Equal(5, list.Count);
            Assert.Equal("Product 6", list[0].ProductName);
            Assert.Equal("Product 10", list[4].ProductName);
        }

        [Fact]
        public async Task GetProductsAsync_WithSearchTerm_ReturnsFilteredProducts()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateDbContext(dbName);
            context.Products.Add(new Product { Id = 1, ProductName = "Apple", CreatedBy = "admin", CreatedOn = DateTime.UtcNow });
            context.Products.Add(new Product { Id = 2, ProductName = "Banana", CreatedBy = "admin", CreatedOn = DateTime.UtcNow });
            context.Products.Add(new Product { Id = 3, ProductName = "Pineapple", CreatedBy = "admin", CreatedOn = DateTime.UtcNow });
            await context.SaveChangesAsync();

            using var uow = new UnitOfWork(context);
            var service = new ProductService(uow);

            // Act
            var (products, totalCount) = await service.GetProductsAsync(pageNumber: 1, pageSize: 10, searchTerm: "apple");

            // Assert
            Assert.Equal(2, totalCount); // Apple, Pineapple
            var list = products.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, p => p.ProductName == "Apple");
            Assert.Contains(list, p => p.ProductName == "Pineapple");
        }

        [Fact]
        public async Task UpdateProductAsync_WithValidId_UpdatesProductName()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateDbContext(dbName);
            var product = new Product { Id = 10, ProductName = "Old Name", CreatedBy = "admin", CreatedOn = DateTime.UtcNow };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            using var uow = new UnitOfWork(context);
            var service = new ProductService(uow);
            var updateDto = new UpdateProductDto { ProductName = "Updated Name" };

            // Act
            var result = await service.UpdateProductAsync(10, updateDto, "modifier");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.ProductName);
            Assert.Equal("modifier", result.ModifiedBy);
            Assert.NotNull(result.ModifiedOn);

            // Verify db is updated
            var dbProduct = await context.Products.FindAsync(10);
            Assert.NotNull(dbProduct);
            Assert.Equal("Updated Name", dbProduct.ProductName);
        }

        [Fact]
        public async Task DeleteProductAsync_WithValidId_DeletesProduct()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateDbContext(dbName);
            var product = new Product { Id = 5, ProductName = "To Delete", CreatedBy = "admin", CreatedOn = DateTime.UtcNow };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            using var uow = new UnitOfWork(context);
            var service = new ProductService(uow);

            // Act
            await service.DeleteProductAsync(5);

            // Assert
            var dbProduct = await context.Products.FindAsync(5);
            Assert.Null(dbProduct);
        }
    }
}
