using Microsoft.EntityFrameworkCore;
using ShopStream.Core.DTOs;
using ShopStream.Core.Entities;
using ShopStream.Core.Interfaces;
using ShopStream.Data;

namespace ShopStream.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(ProductListQuery query);
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request);
    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request);
    Task DeleteProductAsync(Guid id);
}

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        ApplicationDbContext context,
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductListQuery query)
    {
        var queryable = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            queryable = queryable.Where(p => 
                p.Name.Contains(query.Search) || 
                p.Description.Contains(query.Search));
        }

        if (query.CategoryId.HasValue)
        {
            queryable = queryable.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        if (query.MinPrice.HasValue)
        {
            queryable = queryable.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            queryable = queryable.Where(p => p.Price <= query.MaxPrice.Value);
        }

        // Apply sorting
        queryable = query.SortBy.ToLower() switch
        {
            "price" => query.SortDescending 
                ? queryable.OrderByDescending(p => p.Price) 
                : queryable.OrderBy(p => p.Price),
            "createdat" => query.SortDescending 
                ? queryable.OrderByDescending(p => p.CreatedAt) 
                : queryable.OrderBy(p => p.CreatedAt),
            _ => query.SortDescending 
                ? queryable.OrderByDescending(p => p.Name) 
                : queryable.OrderBy(p => p.Name)
        };

        var totalCount = await queryable.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var products = await queryable
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.SKU,
                p.Description,
                p.Price,
                p.StockQuantity,
                p.CategoryId,
                p.Category.Name,
                p.IsActive,
                p.Images.Select(i => new ProductImageDto(i.Id, i.Url, i.AltText, i.DisplayOrder)).ToList()
            ))
            .ToListAsync();

        return new PagedResult<ProductDto>(products, totalCount, query.Page, query.PageSize, totalPages);
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return null;

        return new ProductDto(
            product.Id,
            product.Name,
            product.SKU,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.CategoryId,
            product.Category.Name,
            product.IsActive,
            product.Images.Select(i => new ProductImageDto(i.Id, i.Url, i.AltText, i.DisplayOrder)).ToList()
        );
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            SKU = request.SKU,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return await GetProductByIdAsync(product.Id) 
            ?? throw new InvalidOperationException("Failed to retrieve created product");
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return await GetProductByIdAsync(product.Id) 
            ?? throw new InvalidOperationException("Failed to retrieve updated product");
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found");
        }

        await _productRepository.DeleteAsync(product);
        await _unitOfWork.SaveChangesAsync();
    }
}
