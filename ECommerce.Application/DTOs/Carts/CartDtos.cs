
namespace ECommerce.Application.DTOs.Carts;

public record AddCartItemDto(int ProductId, int Quantity);
public record UpdateCartItemDto(int Quantity);

public record CartItemDto(
    int ProductId,
    string ProductName,
    string Sku,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    int AvailableStock);

public record CartDto(
    List<CartItemDto> Items,
    int TotalItems,
    decimal TotalAmount);