namespace ShopStream.Core.DTOs;

public record AddressDto(
    Guid Id,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country,
    bool IsDefault
);

public record CreateAddressRequest(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country,
    bool IsDefault = false
);
