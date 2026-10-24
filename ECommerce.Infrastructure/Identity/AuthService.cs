using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.WebRequestMethods;

namespace ECommerce.Infrastructure.Identity
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwt;
        private readonly ApplicationDbContext _db;
        private readonly JwtSettings _settings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenGenerator jwt,
            ApplicationDbContext db,
            IOptions<JwtSettings> options,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _jwt = jwt;
            _db = db;
            _settings = options.Value;
            _logger = logger;
        }
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) is not null)
                throw new ConflictException("This email is already registered.");

            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new BadRequestException(
                    string.Join(" ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, Roles.Customer);

            return await IssueTokensAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            var isValid = user is not null && await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!isValid)
            {
                _logger.LogWarning("Failed login attempt. UserId: {UserId}", user?.Id);
                throw new UnauthorizedException("Invalid email or password.");
            }

            return await IssueTokensAsync(user!);
        }

        public async Task<AuthResponseDto> RefreshAsync(RefreshTokenDto dto)
        {
            var hash = RefreshTokenHelper.Hash(dto.RefreshToken);
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (stored is null)
                throw new UnauthorizedException("Invalid refresh token.");

            if (stored.RevokedAtUtc is not null)
            {
                _logger.LogWarning(
                    "Refresh token reuse detected for user {UserId}. Revoking all sessions.", stored.UserId);
                await RevokeAllAsync(stored.UserId);
                throw new UnauthorizedException("Invalid refresh token.");
            }

            if (stored.ExpiresAtUtc <= DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token expired. Please log in again.");

            var user = await _userManager.FindByIdAsync(stored.UserId.ToString())
                ?? throw new UnauthorizedException("Invalid refresh token.");

            return await IssueTokensAsync(user, stored);   // Rotation
        }

        public async Task LogoutAsync(RefreshTokenDto dto)
        {
            var hash = RefreshTokenHelper.Hash(dto.RefreshToken);
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (stored is null || stored.RevokedAtUtc is not null)
                return;               

            stored.RevokedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        private async Task<AuthResponseDto> IssueTokensAsync(ApplicationUser user, RefreshToken? oldToken = null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var (accessToken, expires) = _jwt.Generate(user, roles);

            var rawRefresh = RefreshTokenHelper.Generate();
            var newHash = RefreshTokenHelper.Hash(rawRefresh);

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = newHash,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays)
            });

            if (oldToken is not null)
            {
                oldToken.RevokedAtUtc = DateTime.UtcNow;
                oldToken.ReplacedByTokenHash = newHash;
            }

            await _db.SaveChangesAsync();       

            return new AuthResponseDto(
                user.Id, user.FullName, user.Email!, accessToken, expires, rawRefresh, roles.ToList());
        }

        private async Task RevokeAllAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            await _db.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAtUtc, now));
        }
    }
}