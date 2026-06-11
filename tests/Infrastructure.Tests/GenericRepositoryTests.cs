using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests
{
    public class GenericRepositoryTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddProductToDatabase()
        {
            // Arrange
            var dbContext = GetDbContext("Add_Product_Db");
            var repository = new GenericRepository<Product>(dbContext);
            var product = new Product
            {
                ProductName = "Test Product",
                CreatedBy = "Tester",
                CreatedOn = DateTime.UtcNow
            };

            // Act
            await repository.AddAsync(product);
            await dbContext.SaveChangesAsync();

            // Assert
            var savedProduct = await dbContext.Products.FirstOrDefaultAsync(p => p.ProductName == "Test Product");
            Assert.NotNull(savedProduct);
            Assert.Equal("Test Product", savedProduct.ProductName);
            Assert.Equal("Tester", savedProduct.CreatedBy);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectProduct()
        {
            // Arrange
            var dbContext = GetDbContext("Get_Product_Db");
            var product = new Product
            {
                Id = 1,
                ProductName = "Product 1",
                CreatedBy = "Tester",
                CreatedOn = DateTime.UtcNow
            };
            await dbContext.Products.AddAsync(product);
            await dbContext.SaveChangesAsync();

            var repository = new GenericRepository<Product>(dbContext);

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Product 1", result.ProductName);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllProducts()
        {
            // Arrange
            var dbContext = GetDbContext("GetAll_Product_Db");
            var product1 = new Product { ProductName = "P1", CreatedBy = "Tester", CreatedOn = DateTime.UtcNow };
            var product2 = new Product { ProductName = "P2", CreatedBy = "Tester", CreatedOn = DateTime.UtcNow };
            await dbContext.Products.AddRangeAsync(product1, product2);
            await dbContext.SaveChangesAsync();

            var repository = new GenericRepository<Product>(dbContext);

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task Update_ShouldModifyProductInDatabase()
        {
            // Arrange
            var dbContext = GetDbContext("Update_Product_Db");
            var product = new Product { ProductName = "Original Name", CreatedBy = "Tester", CreatedOn = DateTime.UtcNow };
            await dbContext.Products.AddAsync(product);
            await dbContext.SaveChangesAsync();

            var repository = new GenericRepository<Product>(dbContext);

            // Act
            product.ProductName = "Updated Name";
            repository.Update(product);
            await dbContext.SaveChangesAsync();

            // Assert
            var updatedProduct = await dbContext.Products.FindAsync(product.Id);
            Assert.NotNull(updatedProduct);
            Assert.Equal("Updated Name", updatedProduct.ProductName);
        }

        [Fact]
        public async Task Delete_ShouldRemoveProductFromDatabase()
        {
            // Arrange
            var dbContext = GetDbContext("Delete_Product_Db");
            var product = new Product { ProductName = "To Delete", CreatedBy = "Tester", CreatedOn = DateTime.UtcNow };
            await dbContext.Products.AddAsync(product);
            await dbContext.SaveChangesAsync();

            var repository = new GenericRepository<Product>(dbContext);

            // Act
            repository.Delete(product);
            await dbContext.SaveChangesAsync();

            // Assert
            var deletedProduct = await dbContext.Products.FindAsync(product.Id);
            Assert.Null(deletedProduct);
        }
    }
}
