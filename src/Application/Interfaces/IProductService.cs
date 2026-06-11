using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<(IEnumerable<ProductDto> Products, int TotalCount)> GetProductsAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, string username);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto, string username);
        Task DeleteProductAsync(int id);
    }
}
