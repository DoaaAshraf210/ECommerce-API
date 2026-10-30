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
using System.Buffers.Text;
using System.Collections.Generic;
using System.Net;
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
        private readonly IEmailService _email;
        private readonly AppSettings _app;
        IGoogleTokenValidator _google;
        private const string GoogleProvider = "Google";


        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenGenerator jwt,
            ApplicationDbContext db,
            IOptions<JwtSettings> jwtOptions,
            IEmailService email,
            IOptions<AppSettings> appOptions,
            ILogger<AuthService> logger,
            IGoogleTokenValidator google)
        {
            _userManager = userManager;
            _jwt = jwt;
            _db = db;
            _settings = jwtOptions.Value;
            _email = email;
            _app = appOptions.Value;
            _logger = logger;
            _google = google;
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

            if (user is not null && await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Login blocked: account is locked. UserId: {UserId}", user.Id);
                throw new ForbiddenException("Your account is locked. Please contact support or try again later.");
            }

            var isValid = user is not null && await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!isValid)
            {
                if (user is not null)
                    await _userManager.AccessFailedAsync(user);   // يعدّ المحاولة، ويقفل بعد 5

                _logger.LogWarning("Failed login attempt. UserId: {UserId}", user?.Id);
                throw new UnauthorizedException("Invalid email or password.");
            }

            if (user!.AccessFailedCount > 0)
                await _userManager.ResetAccessFailedCountAsync(user);

            return await IssueTokensAsync(user);
        }

        public async Task<AuthResponseDto> RefreshAsync(RefreshTokenDto dto)
        {
            var hash = RefreshTokenHelper.Hash(dto.RefreshToken);
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (stored is null)
                throw new UnauthorizedException("Invalid refresh token.");

            var user = await _userManager.FindByIdAsync(stored.UserId.ToString());
            if (user is null || await _userManager.IsLockedOutAsync(user))
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

            return await IssueTokensAsync(user, stored);
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

        private Task<int> RevokeAllAsync(Guid userId)
                => RefreshTokenHelper.RevokeAllAsync(_db, userId);
        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
            {
                _logger.LogInformation("Password reset requested for an unregistered email.");
                return;                            
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(token));

            var link = $"{_app.FrontendBaseUrl.TrimEnd('/')}/reset-password" +
                       $"?email={Uri.EscapeDataString(user.Email!)}&token={encodedToken}";

            var body = $"""
        <p>Hello {WebUtility.HtmlEncode(user.FullName)},</p>
        <p>We received a request to reset your password. Click the link below to choose a new one:</p>
        <p><a href="{link}">Reset password</a></p>
        <p>This link expires in 1 hour. If you didn't request this, you can ignore this email.</p>
        """;

            await _email.SendAsync(user.Email!, "Reset your password", body);

            _logger.LogInformation("Password reset email sent. UserId: {UserId}", user.Id);
        }


        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            const string invalidMessage = "Invalid or expired reset link.";

            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new BadRequestException(invalidMessage);

            string token;
            try
            {
                token = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(dto.Token));
            }
            catch (FormatException)
            {
                throw new BadRequestException(invalidMessage);
            }

            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.InvalidToken)))
                    throw new BadRequestException(invalidMessage);

                throw new BadRequestException(
                    string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            await RevokeAllAsync(user.Id);

            _logger.LogInformation("Password reset completed. UserId: {UserId}", user.Id);
        }



        public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto)
        {
            var info = await _google.ValidateAsync(dto.IdToken)
                ?? throw new UnauthorizedException("Invalid Google token.");

            if (!info.EmailVerified)
                throw new UnauthorizedException("Your Google email is not verified.");

            var user = await _userManager.FindByLoginAsync(GoogleProvider, info.Subject);

            if (user is null)
            {
                user = await _userManager.FindByEmailAsync(info.Email);

                if (user is null)
                {
                    user = new ApplicationUser
                    {
                        FullName = string.IsNullOrWhiteSpace(info.Name)
                            ? info.Email.Split('@')[0]
                            : info.Name,
                        Email = info.Email,
                        UserName = info.Email,
                        EmailConfirmed = true
                    };

                    var created = await _userManager.CreateAsync(user);
                    if (!created.Succeeded)
                        throw new BadRequestException(
                            string.Join(" ", created.Errors.Select(e => e.Description)));

                    await _userManager.AddToRoleAsync(user, Roles.Customer);
                }
                else if (!user.EmailConfirmed)
                {
                    _logger.LogWarning(
                        "Linking Google to an unconfirmed account. Removing password. UserId: {UserId}", user.Id);

                    if (await _userManager.HasPasswordAsync(user))
                        await _userManager.RemovePasswordAsync(user);

                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    await RevokeAllAsync(user.Id);
                }

                var linked = await _userManager.AddLoginAsync(
                    user, new UserLoginInfo(GoogleProvider, info.Subject, GoogleProvider));

                if (!linked.Succeeded)
                    throw new BadRequestException(
                        string.Join(" ", linked.Errors.Select(e => e.Description)));

                _logger.LogInformation("Google account linked. UserId: {UserId}", user.Id);
            }
            if (await _userManager.IsLockedOutAsync(user))
                throw new ForbiddenException("Your account is locked. Please contact support or try again later.");

            return await IssueTokensAsync(user);
        }


    }
}