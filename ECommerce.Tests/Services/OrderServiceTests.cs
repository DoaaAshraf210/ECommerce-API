using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.ValueObjects;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Tests.Services
{
    public class OrderServiceTests
    {

        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<ICartRepository> _carts = new();
        private readonly Mock<IOrderRepository> _orders = new();
        private readonly Mock<IProductRepository> _products = new();
        private readonly OrderService _sut;

        public OrderServiceTests()
        {
            _uow.Setup(u => u.Carts).Returns(_carts.Object);
            _uow.Setup(u => u.Orders).Returns(_orders.Object);
            _uow.Setup(u => u.Products).Returns(_products.Object);

            _sut = new OrderService(_uow.Object);
        }

        private static CheckoutDto ValidCheckout() =>
            new(new ShippingAddressDto("Doaa", "01012345678", "Street 1", "Minya", null));

        private static Product NewProduct(int id = 1, int stock = 10, decimal price = 100m) =>
            new() { Id = id, Name = "Phone", Sku = $"PH-{id}", Price = price, StockQuantity = stock, IsActive = true };


        private static Order NewOrder(Guid userId, OrderStatus status, int productId = 1, int quantity = 3)
        {
            var order = new Order
            {
                Id = 5,
                UserId = userId,
                OrderNumber = "ORD-TEST",
                Status = status,
                ShippingAddress = new ShippingAddress
                {
                    FullName = "x",
                    Phone = "x",
                    Street = "x",
                    City = "x"
                }
            };

            order.Items.Add(new OrderItem
            {
                ProductId = productId,
                ProductName = "Phone",
                Sku = "PH-1",
                UnitPrice = 100m,
                Quantity = quantity
            });

            return order;
        }
        [Fact]
        public async Task CheckoutAsync_CartIsEmpty_ThrowsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _carts.Setup(c => c.GetByUserIdWithItemsAsync(userId))
                  .ReturnsAsync(new Cart { UserId = userId });

            // Act + Assert
            await Assert.ThrowsAsync<BadRequestException>(
                () => _sut.CheckoutAsync(userId, ValidCheckout()));

            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CheckoutAsync_ValidCart_CreatesOrderDecreasesStockAndClearsCart()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var product = NewProduct(stock: 10, price: 100m);
            var cart = new Cart { UserId = userId };
            cart.Items.Add(new CartItem { ProductId = product.Id, Product = product, Quantity = 3 });

            _carts.Setup(c => c.GetByUserIdWithItemsAsync(userId)).ReturnsAsync(cart);

            // Act
            var result = await _sut.CheckoutAsync(userId, ValidCheckout());

            // Assert
            Assert.Equal(7, product.StockQuantity);
            Assert.Empty(cart.Items);
            Assert.Equal(300m, result.TotalAmount);
            Assert.Equal("Pending", result.Status);

            _orders.Verify(o => o.AddAsync(It.IsAny<Order>()), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CheckoutAsync_QuantityExceedsStock_ThrowsConflict()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var product = NewProduct(stock: 3);
            var cart = new Cart { UserId = userId };
            cart.Items.Add(new CartItem { ProductId = product.Id, Product = product, Quantity = 5 });

            _carts.Setup(c => c.GetByUserIdWithItemsAsync(userId)).ReturnsAsync(cart);

            // Act + Assert
            await Assert.ThrowsAsync<ConflictException>(
                () => _sut.CheckoutAsync(userId, ValidCheckout()));

            Assert.Equal(3, product.StockQuantity);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CheckoutAsync_InactiveProduct_ThrowsBadRequest()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var product = NewProduct(stock: 10);
            product.IsActive = false;
            var cart = new Cart { UserId = userId };
            cart.Items.Add(new CartItem { ProductId = product.Id, Product = product, Quantity = 1 });

            _carts.Setup(c => c.GetByUserIdWithItemsAsync(userId)).ReturnsAsync(cart);

            // Act + Assert
            await Assert.ThrowsAsync<BadRequestException>(
                () => _sut.CheckoutAsync(userId, ValidCheckout()));

            Assert.Equal(10, product.StockQuantity);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CancelMyOrderAsync_PendingOrder_RestoresStock()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var product = NewProduct(id: 1, stock: 7);
            var order = NewOrder(userId, OrderStatus.Pending, productId: 1, quantity: 3);

            _orders.Setup(o => o.GetByIdWithItemsAsync(5)).ReturnsAsync(order);
            _products.Setup(p => p.GetByIdsAsync(It.IsAny<IReadOnlyCollection<int>>()))
                     .ReturnsAsync(new List<Product> { product });

            // Act
            var result = await _sut.CancelMyOrderAsync(userId, 5);

            // Assert
            Assert.Equal(10, product.StockQuantity);      // 7 + 3
            Assert.Equal("Cancelled", result.Status);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CancelMyOrderAsync_PaidOrder_ThrowsConflict()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = NewOrder(userId, OrderStatus.Paid);

            _orders.Setup(o => o.GetByIdWithItemsAsync(5)).ReturnsAsync(order);

            // Act + Assert
            await Assert.ThrowsAsync<ConflictException>(
                () => _sut.CancelMyOrderAsync(userId, 5));

            Assert.Equal(OrderStatus.Paid, order.Status);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateStatusAsync_InvalidTransition_ThrowsConflict()
        {
            // Arrange
            var order = NewOrder(Guid.NewGuid(), OrderStatus.Shipped);

            _orders.Setup(o => o.GetByIdWithItemsAsync(5)).ReturnsAsync(order);

            // Act + Assert
            await Assert.ThrowsAsync<ConflictException>(
                () => _sut.UpdateStatusAsync(5, new UpdateOrderStatusDto(OrderStatus.Pending)));

            Assert.Equal(OrderStatus.Shipped, order.Status);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        [Fact]
        public async Task GetMyOrderAsync_OrderBelongsToAnotherUser_ThrowsNotFound()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var strangerId = Guid.NewGuid();
            var order = NewOrder(ownerId, OrderStatus.Pending);

            _orders.Setup(o => o.GetByIdWithItemsAsync(5)).ReturnsAsync(order);

            // Act + Assert
            await Assert.ThrowsAsync<NotFoundException>(
                () => _sut.GetMyOrderAsync(strangerId, 5));
        }

    }
}