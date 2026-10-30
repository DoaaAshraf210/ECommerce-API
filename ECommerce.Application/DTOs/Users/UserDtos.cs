namespace ECommerce.Application.DTOs.Users;

public record AdminUserListItemDto(
    Guid Id, string FullName, string Email, bool EmailConfirmed,
    bool IsLocked, List<string> Roles, DateTime CreatedAt);

public record AdminUserDetailsDto(
    Guid Id, string FullName, string Email, bool EmailConfirmed,
    bool IsLocked, DateTimeOffset? LockoutEnd, List<string> Roles,
    List<string> LoginProviders, bool HasPassword, int OrdersCount, DateTime CreatedAt);

public record SetUserRoleDto(string Role);

public class UserQueryParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsLocked { get; set; }
}