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
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ItemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ItemDto> GetItemByIdAsync(int id)
        {
            var item = await _unitOfWork.Items.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                throw new NotFoundException(nameof(Item), id);
            }

            return item.ToDto();
        }

        public async Task<IEnumerable<ItemDto>> GetItemsByProductIdAsync(int productId)
        {
            // Verify product exists
            var productExists = await _unitOfWork.Products.GetQueryable()
                .AnyAsync(p => p.Id == productId);

            if (!productExists)
            {
                throw new NotFoundException(nameof(Product), productId);
            }

            var items = await _unitOfWork.Items.GetQueryable()
                .AsNoTracking()
                .Where(i => i.ProductId == productId)
                .ToListAsync();

            return items.Select(i => i.ToDto());
        }

        public async Task<ItemDto> AddItemToProductAsync(int productId, CreateItemDto createItemDto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundException(nameof(Product), productId);
            }

            var item = new Item
            {
                ProductId = productId,
                Quantity = createItemDto.Quantity
            };

            await _unitOfWork.Items.AddAsync(item);
            await _unitOfWork.SaveChangesAsync();

            return item.ToDto();
        }

        public async Task<ItemDto> UpdateItemAsync(int id, UpdateItemDto updateItemDto)
        {
            var item = await _unitOfWork.Items.GetByIdAsync(id);
            if (item == null)
            {
                throw new NotFoundException(nameof(Item), id);
            }

            item.Quantity = updateItemDto.Quantity;

            _unitOfWork.Items.Update(item);
            await _unitOfWork.SaveChangesAsync();

            return item.ToDto();
        }

        public async Task DeleteItemAsync(int id)
        {
            var item = await _unitOfWork.Items.GetByIdAsync(id);
            if (item == null)
            {
                throw new NotFoundException(nameof(Item), id);
            }

            _unitOfWork.Items.Delete(item);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
