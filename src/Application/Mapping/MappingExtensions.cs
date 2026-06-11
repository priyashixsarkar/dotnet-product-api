using System.Linq;
using Domain.Entities;
using Application.DTOs;

namespace Application.Mapping
{
    public static class MappingExtensions
    {
        public static ProductDto ToDto(this Product product)
        {
            if (product == null) return null!;
            return new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                CreatedBy = product.CreatedBy,
                CreatedOn = product.CreatedOn,
                ModifiedBy = product.ModifiedBy,
                ModifiedOn = product.ModifiedOn,
                Items = product.Items != null 
                    ? product.Items.Select(i => i.ToDto()).ToList() 
                    : new System.Collections.Generic.List<ItemDto>()
            };
        }

        public static ItemDto ToDto(this Item item)
        {
            if (item == null) return null!;
            return new ItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity
            };
        }
    }
}
