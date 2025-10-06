namespace ShopStream.Core.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    string? ParentCategoryName
);

public record CreateCategoryRequest(
    string Name,
    string? Description,
    Guid? ParentCategoryId
);
