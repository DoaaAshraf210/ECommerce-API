using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Orders;

public record ShippingAddressDto(
    string FullName, string Phone, string Street, string City, string? PostalCode);

public record CheckoutDto(ShippingAddressDto ShippingAddress);

public record OrderItemDto(
    int ProductId, string ProductName, string Sku,
    decimal UnitPrice, int Quantity, decimal LineTotal);

public record OrderDto(
    int Id, string OrderNumber, string Status, decimal TotalAmount,
    DateTime CreatedAt, ShippingAddressDto ShippingAddress, List<OrderItemDto> Items);

public record OrderSummaryDto(
    int Id, string OrderNumber, string Status, decimal TotalAmount,
    int ItemsCount, DateTime CreatedAt);

public record UpdateOrderStatusDto(OrderStatus Status);

public class OrderQueryParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public OrderStatus? Status { get; set; }
}