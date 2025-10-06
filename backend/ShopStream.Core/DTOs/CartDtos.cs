namespace ShopStream.Core.DTOs;

public record CartDto(
    Guid Id,
    List<CartItemDto> Items,
    decimal TotalAmount
);

public record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? ImageUrl
);

public record AddToCartRequest(
    Guid ProductId,
    int Quantity
);

public record UpdateCartItemRequest(
    int Quantity
);
