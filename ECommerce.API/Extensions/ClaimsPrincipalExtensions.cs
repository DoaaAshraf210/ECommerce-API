using ECommerce.Application.Common.Exceptions;
using System.Security.Claims;

namespace ECommerce.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirst("sub")?.Value;

            return Guid.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedException("Invalid token.");
        }
    }
}
