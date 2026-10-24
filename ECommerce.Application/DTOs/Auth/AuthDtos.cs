
namespace ECommerce.Application.DTOs.Auth;

public record RegisterDto(string FullName, string Email, string Password);
public record LoginDto(string Email, string Password);


public record RefreshTokenDto(string RefreshToken);

public record AuthResponseDto(
    Guid UserId,
    string FullName,
    string Email,
    string Token,
    DateTime ExpiresAtUtc,
    string RefreshToken,
    List<string> Roles);