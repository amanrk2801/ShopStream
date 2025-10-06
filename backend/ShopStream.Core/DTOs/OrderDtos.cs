namespace ShopStream.Core.DTOs;

public record OrderDto(
    Guid Id,
    string OrderNumber,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    AddressDto? ShippingAddress,
    List<OrderItemDto> Items,
    PaymentDto? Payment
);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);

public record CreateOrderRequest(
    Guid ShippingAddressId,
    string PaymentProvider = "Mock"
);

public record PaymentDto(
    Guid Id,
    string Provider,
    string Status,
    string? TransactionId,
    decimal Amount
);
