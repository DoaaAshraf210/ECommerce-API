using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Interfaces
{
    public interface IUserAdminService
    {
        Task<PagedResult<AdminUserListItemDto>> GetUsersAsync(UserQueryParams query);
        Task<AdminUserDetailsDto> GetUserAsync(Guid id);
        Task LockAsync(Guid actingUserId, Guid id);
        Task UnlockAsync(Guid actingUserId, Guid id);
        Task SetRoleAsync(Guid actingUserId, Guid id, string role);
    }
}
