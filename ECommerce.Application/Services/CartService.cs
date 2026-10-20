using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Carts;
using ECommerce.Application.Mappings;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _uow;

        public CartService(IUnitOfWork uow) => _uow = uow;

        public async Task<CartDto> GetCartAsync(Guid userId)
        {
            var cart = await _uow.Carts.GetByUserIdWithItemsAsync(userId);
            return cart is null ? new CartDto(new(), 0, 0m) : cart.ToDto();
        }

        public async Task<CartDto> AddItemAsync(Guid userId, AddCartItemDto dto)
        {
            var product = await _uow.Products.GetByIdWithDetailsAsync(dto.ProductId)
                ?? throw new NotFoundException("Product", dto.ProductId);

            if (!product.IsActive)
                throw new BadRequestException("This product is not available.");

            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);

            var newQuantity = (item?.Quantity ?? 0) + dto.Quantity;
            EnsureStock(product, newQuantity);

            if (item is null)
                cart.Items.Add(new CartItem { ProductId = product.Id, Product = product, Quantity = newQuantity });
            else
                item.Quantity = newQuantity;

            await _uow.SaveChangesAsync();
            return cart.ToDto();
        }

        public async Task<CartDto> UpdateItemAsync(Guid userId, int productId, UpdateCartItemDto dto)
        {
            var (cart, item) = await GetItemAsync(userId, productId);

            EnsureStock(item.Product, dto.Quantity);
            item.Quantity = dto.Quantity;

            await _uow.SaveChangesAsync();
            return cart.ToDto();
        }

        public async Task<CartDto> RemoveItemAsync(Guid userId, int productId)
        {
            var (cart, item) = await GetItemAsync(userId, productId);

            cart.Items.Remove(item);

            await _uow.SaveChangesAsync();
            return cart.ToDto();
        }

        public async Task ClearAsync(Guid userId)
        {
            var cart = await _uow.Carts.GetByUserIdWithItemsAsync(userId);
            if (cart is null) return;

            cart.Items.Clear();
            await _uow.SaveChangesAsync();
        }


        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            var cart = await _uow.Carts.GetByUserIdWithItemsAsync(userId);
            if (cart is not null) return cart;

            cart = new Cart { UserId = userId };
            await _uow.Carts.AddAsync(cart);
            return cart;
        }

        private async Task<(Cart Cart, CartItem Item)> GetItemAsync(Guid userId, int productId)
        {
            var cart = await _uow.Carts.GetByUserIdWithItemsAsync(userId)
                ?? throw new NotFoundException("Cart item", productId);

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId)
                ?? throw new NotFoundException("Cart item", productId);

            return (cart, item);
        }

        private static void EnsureStock(Product product, int quantity)
        {
            if (quantity > product.StockQuantity)
                throw new ConflictException(
                    $"Only {product.StockQuantity} unit(s) of '{product.Name}' available.");
        }
    }
}