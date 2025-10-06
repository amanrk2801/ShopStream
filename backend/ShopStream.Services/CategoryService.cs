using Microsoft.EntityFrameworkCore;
using ShopStream.Core.DTOs;
using ShopStream.Core.Entities;
using ShopStream.Core.Interfaces;
using ShopStream.Data;

namespace ShopStream.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
    Task DeleteCategoryAsync(Guid id);
}

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(ApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.ParentCategory)
            .ToListAsync();

        return categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.ParentCategoryId,
            c.ParentCategory?.Name
        )).ToList();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _context.Categories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return null;

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.ParentCategoryId,
            category.ParentCategory?.Name
        );
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return await GetCategoryByIdAsync(category.Id) 
            ?? throw new InvalidOperationException("Failed to retrieve created category");
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {id} not found");
        }

        _context.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync();
    }
}
