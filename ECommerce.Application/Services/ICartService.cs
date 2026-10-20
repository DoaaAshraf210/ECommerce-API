using ECommerce.Application.DTOs.Carts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Services
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(Guid userId);
        Task<CartDto> AddItemAsync(Guid userId, AddCartItemDto dto);
        Task<CartDto> UpdateItemAsync(Guid userId, int productId, UpdateCartItemDto dto);
        Task<CartDto> RemoveItemAsync(Guid userId, int productId);
        Task ClearAsync(Guid userId);
    }
}
