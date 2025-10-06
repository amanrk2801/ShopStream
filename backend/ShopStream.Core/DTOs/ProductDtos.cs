namespace ShopStream.Core.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string SKU,
    string Description,
    decimal Price,
    int StockQuantity,
    Guid CategoryId,
    string CategoryName,
    bool IsActive,
    List<ProductImageDto> Images
);

public record ProductImageDto(
    Guid Id,
    string Url,
    string AltText,
    int DisplayOrder
);

public record CreateProductRequest(
    string Name,
    string SKU,
    string Description,
    decimal Price,
    int StockQuantity,
    Guid CategoryId
);

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    Guid CategoryId,
    bool IsActive
);

public record ProductListQuery(
    string? Search = null,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "Name",
    bool SortDescending = false
);

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
