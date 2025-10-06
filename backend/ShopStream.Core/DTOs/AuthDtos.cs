namespace ShopStream.Core.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName
);

public record LoginRequest(
    string Email,
    string Password
);

public record LoginResponse(
    string Token,
    string Email,
    string FirstName,
    string LastName,
    string Role
);

public record PasswordResetRequest(
    string Email
);

public record PasswordResetConfirm(
    string Token,
    string NewPassword
);
