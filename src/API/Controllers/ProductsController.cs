using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Application.DTOs;
using Application.Interfaces;

namespace API.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/products")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IItemService _itemService;

        public ProductsController(IProductService productService, IItemService itemService)
        {
            _productService = productService;
            _itemService = itemService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 10;

            var (products, totalCount) = await _productService.GetProductsAsync(pageNumber, pageSize, searchTerm);
            
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return Ok(new
            {
                items = products,
                pageNumber,
                pageSize,
                totalCount,
                totalPages
            });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto createProductDto)
        {
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            var product = await _productService.CreateProductAsync(createProductDto, username);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "System";
            var product = await _productService.UpdateProductAsync(id, updateProductDto, username);
            return Ok(product);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }

        // --- Nested Items Endpoints ---

        [HttpGet("{productId}/items")]
        [AllowAnonymous]
        public async Task<IActionResult> GetItems(int productId)
        {
            var items = await _itemService.GetItemsByProductIdAsync(productId);
            return Ok(items);
        }

        [HttpPost("{productId}/items")]
        public async Task<IActionResult> AddItem(int productId, [FromBody] CreateItemDto createItemDto)
        {
            var item = await _itemService.AddItemToProductAsync(productId, createItemDto);
            return CreatedAtAction(nameof(GetItemById), new { productId, id = item.Id }, item);
        }

        [HttpGet("{productId}/items/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetItemById(int productId, int id)
        {
            var item = await _itemService.GetItemByIdAsync(id);
            if (item.ProductId != productId)
            {
                return BadRequest("Item does not belong to the specified product.");
            }
            return Ok(item);
        }

        [HttpPut("{productId}/items/{id}")]
        public async Task<IActionResult> UpdateItem(int productId, int id, [FromBody] UpdateItemDto updateItemDto)
        {
            // Verify item belongs to product
            var item = await _itemService.GetItemByIdAsync(id);
            if (item.ProductId != productId)
            {
                return BadRequest("Item does not belong to the specified product.");
            }

            var updatedItem = await _itemService.UpdateItemAsync(id, updateItemDto);
            return Ok(updatedItem);
        }

        [HttpDelete("{productId}/items/{id}")]
        public async Task<IActionResult> DeleteItem(int productId, int id)
        {
            // Verify item belongs to product
            var item = await _itemService.GetItemByIdAsync(id);
            if (item.ProductId != productId)
            {
                return BadRequest("Item does not belong to the specified product.");
            }

            await _itemService.DeleteItemAsync(id);
            return NoContent();
        }
    }
}
