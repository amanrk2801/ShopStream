using Microsoft.EntityFrameworkCore;
using ShopStream.Core.DTOs;
using ShopStream.Core.Entities;
using ShopStream.Core.Interfaces;
using ShopStream.Data;

namespace ShopStream.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid userId);
    Task<CartDto> AddToCartAsync(Guid userId, AddToCartRequest request);
    Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request);
    Task RemoveFromCartAsync(Guid userId, Guid cartItemId);
    Task ClearCartAsync(Guid userId);
}

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(ApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<CartDto> GetCartAsync(Guid userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return MapToDto(cart);
    }

    public async Task<CartDto> AddToCartAsync(Guid userId, AddToCartRequest request)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var product = await _context.Products.FindAsync(request.ProductId);

        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {request.ProductId} not found");
        }

        if (product.StockQuantity < request.Quantity)
        {
            throw new InvalidOperationException("Insufficient stock");
        }

        // Check if item already exists in cart
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        
        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.UnitPrice = product.Price;
        }
        else
        {
            var newItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                UnitPrice = product.Price,
                CreatedAt = DateTime.UtcNow
            };
            cart.Items.Add(newItem);
            await _context.CartItems.AddAsync(newItem);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync();

        // Reload cart with fresh data
        return await GetCartAsync(userId);
    }

    public async Task<CartDto> UpdateCartItemAsync(Guid userId, Guid cartItemId, UpdateCartItemRequest request)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

        if (cartItem == null)
        {
            throw new KeyNotFoundException($"Cart item with ID {cartItemId} not found");
        }

        if (request.Quantity <= 0)
        {
            cart.Items.Remove(cartItem);
        }
        else
        {
            var product = await _context.Products.FindAsync(cartItem.ProductId);
            if (product != null && product.StockQuantity < request.Quantity)
            {
                throw new InvalidOperationException("Insufficient stock");
            }

            cartItem.Quantity = request.Quantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(await GetOrCreateCartAsync(userId));
    }

    public async Task RemoveFromCartAsync(Guid userId, Guid cartItemId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var cartItem = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

        if (cartItem != null)
        {
            cart.Items.Remove(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task ClearCartAsync(Guid userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        cart.Items.Clear();
        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Carts.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();
        }

        return cart;
    }

    private static CartDto MapToDto(Cart cart)
    {
        var items = cart.Items.Select(i => new CartItemDto(
            i.Id,
            i.ProductId,
            i.Product.Name,
            i.Quantity,
            i.UnitPrice,
            i.Quantity * i.UnitPrice,
            i.Product.Images.FirstOrDefault()?.Url
        )).ToList();

        var totalAmount = items.Sum(i => i.TotalPrice);

        return new CartDto(cart.Id, items, totalAmount);
    }
}
