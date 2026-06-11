using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Application.DTOs;
using Application.Interfaces;
using Application.Mapping;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new NotFoundException(nameof(Product), id);
            }

            return product.ToDto();
        }

        public async Task<(IEnumerable<ProductDto> Products, int TotalCount)> GetProductsAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Include(p => p.Items)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.ProductName.ToLower().Contains(searchTerm.ToLower()));
            }

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products.Select(p => p.ToDto()), totalCount);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, string username)
        {
            var product = new Product
            {
                ProductName = createProductDto.ProductName,
                CreatedBy = username,
                CreatedOn = DateTime.UtcNow
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return product.ToDto();
        }

        public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto, string username)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);

            if (product == null)
            {
                throw new NotFoundException(nameof(Product), id);
            }

            product.ProductName = updateProductDto.ProductName;
            product.ModifiedBy = username;
            product.ModifiedOn = DateTime.UtcNow;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            // Load items for the returned DTO representation
            await _unitOfWork.Items.GetQueryable()
                .Where(i => i.ProductId == id)
                .LoadAsync();

            return product.ToDto();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);

            if (product == null)
            {
                throw new NotFoundException(nameof(Product), id);
            }

            _unitOfWork.Products.Delete(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
