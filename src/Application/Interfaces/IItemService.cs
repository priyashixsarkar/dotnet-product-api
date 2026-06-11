using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IItemService
    {
        Task<ItemDto> GetItemByIdAsync(int id);
        Task<IEnumerable<ItemDto>> GetItemsByProductIdAsync(int productId);
        Task<ItemDto> AddItemToProductAsync(int productId, CreateItemDto createItemDto);
        Task<ItemDto> UpdateItemAsync(int id, UpdateItemDto updateItemDto);
        Task DeleteItemAsync(int id);
    }
}
