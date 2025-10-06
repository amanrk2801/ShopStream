using Microsoft.EntityFrameworkCore;
using ShopStream.Core.DTOs;
using ShopStream.Core.Entities;
using ShopStream.Core.Interfaces;
using ShopStream.Data;

namespace ShopStream.Services;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderRequest request);
    Task<OrderDto?> GetOrderByIdAsync(Guid userId, Guid orderId);
    Task<List<OrderDto>> GetUserOrdersAsync(Guid userId);
    Task<List<OrderDto>> GetAllOrdersAsync(); // Admin only
    Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
}

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;

    public OrderService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IPaymentService paymentService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
    }

    public async Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Get user's cart
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                throw new InvalidOperationException("Cart is empty");
            }

            // Validate stock availability
            foreach (var item in cart.Items)
            {
                if (item.Product.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product: {item.Product.Name}");
                }
            }

            // Create order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                ShippingAddressId = request.ShippingAddressId,
                TotalAmount = cart.Items.Sum(i => i.Quantity * i.UnitPrice),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Create order items and update stock
            foreach (var cartItem in cart.Items)
            {
                order.Items.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    CreatedAt = DateTime.UtcNow
                });

                // Reduce stock
                cartItem.Product.StockQuantity -= cartItem.Quantity;
            }

            await _context.Orders.AddAsync(order);
            
            // Clear cart
            _context.CartItems.RemoveRange(cart.Items);

            await _unitOfWork.SaveChangesAsync();

            // Process payment
            var payment = await _paymentService.ProcessPaymentAsync(order.Id, order.TotalAmount, request.PaymentProvider);
            
            if (payment.Status == PaymentStatus.Completed)
            {
                order.Status = OrderStatus.PaymentReceived;
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitTransactionAsync();

            return await GetOrderByIdAsync(userId, order.Id) 
                ?? throw new InvalidOperationException("Failed to retrieve created order");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid userId, Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order == null ? null : MapToDto(order);
    }

    public async Task<List<OrderDto>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Payment)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<List<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} not found");
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(order);
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.OrderNumber,
            order.TotalAmount,
            order.Status.ToString(),
            order.CreatedAt,
            order.ShippingAddress == null ? null : new AddressDto(
                order.ShippingAddress.Id,
                order.ShippingAddress.Street,
                order.ShippingAddress.City,
                order.ShippingAddress.State,
                order.ShippingAddress.ZipCode,
                order.ShippingAddress.Country,
                order.ShippingAddress.IsDefault
            ),
            order.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.Product.Name,
                i.Quantity,
                i.UnitPrice,
                i.Quantity * i.UnitPrice
            )).ToList(),
            order.Payment == null ? null : new PaymentDto(
                order.Payment.Id,
                order.Payment.Provider,
                order.Payment.Status.ToString(),
                order.Payment.TransactionId,
                order.Payment.Amount
            )
        );
    }
}
