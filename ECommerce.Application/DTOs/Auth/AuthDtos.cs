
namespace ECommerce.Application.DTOs.Auth;

public record RegisterDto(string FullName, string Email, string Password);
public record LoginDto(string Email, string Password);

public record AuthResponseDto(
    Guid UserId,
    string FullName,
    string Email,
    string Token,
    DateTime ExpiresAtUtc,
    List<string> Roles);