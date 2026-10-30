using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Users;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Identity
{
    public class UserAdminService : IUserAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<UserAdminService> _logger;

        public UserAdminService(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            ILogger<UserAdminService> logger)
        {
            _userManager = userManager;
            _db = db;
            _logger = logger;
        }

        // ================= القراءة =================

        public async Task<PagedResult<AdminUserListItemDto>> GetUsersAsync(UserQueryParams q)
        {
            var pageNumber = Math.Max(1, q.PageNumber);
            var pageSize = Math.Clamp(q.PageSize, 1, 50);
            var now = DateTimeOffset.UtcNow;

            var query = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.Trim();
                query = query.Where(u =>
                    u.FullName.Contains(s) || (u.Email != null && u.Email.Contains(s)));
            }

            if (q.IsLocked is true)
                query = query.Where(u => u.LockoutEnd != null && u.LockoutEnd > now);
            else if (q.IsLocked is false)
                query = query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= now);

            if (!string.IsNullOrWhiteSpace(q.Role))
            {
                var roleId = await _db.Roles
                    .Where(r => r.Name == q.Role)
                    .Select(r => (Guid?)r.Id)
                    .FirstOrDefaultAsync();

                if (roleId is null)
                    return new PagedResult<AdminUserListItemDto>(new List<AdminUserListItemDto>(), 0, pageNumber, pageSize);

                query = query.Where(u => _db.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == roleId.Value));
            }

            var total = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ThenBy(u => u.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new { u.Id, u.FullName, u.Email, u.EmailConfirmed, u.LockoutEnd, u.CreatedAt })
                .ToListAsync();

            // أدوار الصفحة كلها في Query واحدة (من غير N+1)
            var ids = users.Select(u => u.Id).ToList();
            var roleRows = await (
                    from ur in _db.UserRoles
                    join r in _db.Roles on ur.RoleId equals r.Id
                    where ids.Contains(ur.UserId)
                    select new { ur.UserId, r.Name })
                .ToListAsync();

            var rolesByUser = roleRows.ToLookup(x => x.UserId, x => x.Name ?? "");

            var items = users.Select(u => new AdminUserListItemDto(
                    u.Id,
                    u.FullName,
                    u.Email ?? "",
                    u.EmailConfirmed,
                    u.LockoutEnd != null && u.LockoutEnd > now,
                    rolesByUser[u.Id].ToList(),
                    u.CreatedAt))
                .ToList();

            return new PagedResult<AdminUserListItemDto>(items, total, pageNumber, pageSize);
        }

        public async Task<AdminUserDetailsDto> GetUserAsync(Guid id)
        {
            var user = await FindAsync(id);

            var roles = await _userManager.GetRolesAsync(user);
            var logins = await _userManager.GetLoginsAsync(user);
            var hasPassword = await _userManager.HasPasswordAsync(user);
            var ordersCount = await _db.Orders.CountAsync(o => o.UserId == id);

            return new AdminUserDetailsDto(
                user.Id, user.FullName, user.Email ?? "", user.EmailConfirmed,
                IsLocked(user), user.LockoutEnd, roles.ToList(),
                logins.Select(l => l.LoginProvider).ToList(),
                hasPassword, ordersCount, user.CreatedAt);
        }

        // ================= الإجراءات =================

        public async Task LockAsync(Guid actingUserId, Guid id)
        {
            if (actingUserId == id)
                throw new ConflictException("You cannot lock your own account.");

            var user = await FindAsync(id);

            if (await IsActiveAdminAsync(user) && await CountActiveAdminsAsync() <= 1)
                throw new ConflictException("Cannot lock the last active administrator.");

            await _userManager.SetLockoutEnabledAsync(user, true);
            ThrowIfFailed(await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue));
            await _userManager.UpdateSecurityStampAsync(user);
            await RefreshTokenHelper.RevokeAllAsync(_db, user.Id);

            _logger.LogInformation("Admin {AdminId} locked user {UserId}", actingUserId, id);
        }

        public async Task UnlockAsync(Guid actingUserId, Guid id)
        {
            var user = await FindAsync(id);

            ThrowIfFailed(await _userManager.SetLockoutEndDateAsync(user, null));
            await _userManager.ResetAccessFailedCountAsync(user);

            _logger.LogInformation("Admin {AdminId} unlocked user {UserId}", actingUserId, id);
        }

        public async Task SetRoleAsync(Guid actingUserId, Guid id, string role)
        {
            if (role is not (Roles.Admin or Roles.Customer))
                throw new BadRequestException("Unknown role.");

            var user = await FindAsync(id);
            var current = await _userManager.GetRolesAsync(user);

            if (current.Count == 1 && current[0] == role)
                return;                                  // مفيش تغيير (Idempotent)

            var removingAdmin = current.Contains(Roles.Admin) && role != Roles.Admin;

            if (removingAdmin && actingUserId == id)
                throw new ConflictException("You cannot remove your own administrator role.");

            if (removingAdmin && !IsLocked(user) && await CountActiveAdminsAsync() <= 1)
                throw new ConflictException("Cannot demote the last active administrator.");

            await using var tx = await _db.Database.BeginTransactionAsync();

            var toRemove = current.Where(r => r != role).ToList();
            if (toRemove.Count > 0)
                ThrowIfFailed(await _userManager.RemoveFromRolesAsync(user, toRemove));

            if (!current.Contains(role))
                ThrowIfFailed(await _userManager.AddToRoleAsync(user, role));

            await _userManager.UpdateSecurityStampAsync(user);
            await RefreshTokenHelper.RevokeAllAsync(_db, user.Id);

            await tx.CommitAsync();

            _logger.LogInformation(
                "Admin {AdminId} changed role of user {UserId} to {Role}", actingUserId, id, role);
        }

        // ================= helpers =================

        private async Task<ApplicationUser> FindAsync(Guid id)
            => await _userManager.FindByIdAsync(id.ToString())
               ?? throw new NotFoundException("User", id);

        private static bool IsLocked(ApplicationUser u)
            => u.LockoutEnd is not null && u.LockoutEnd > DateTimeOffset.UtcNow;

        private async Task<bool> IsActiveAdminAsync(ApplicationUser u)
            => !IsLocked(u) && await _userManager.IsInRoleAsync(u, Roles.Admin);

        private async Task<int> CountActiveAdminsAsync()
        {
            var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
            return admins.Count(a => !IsLocked(a));
        }

        private static void ThrowIfFailed(IdentityResult result)
        {
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}