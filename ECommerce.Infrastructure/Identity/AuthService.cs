using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Identity
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwt;

        public AuthService(UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwt)
        {
            _userManager = userManager;
            _jwt = jwt;
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

            return await BuildResponseAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedException("Invalid email or password.");

            return await BuildResponseAsync(user);
        }

        private async Task<AuthResponseDto> BuildResponseAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var (token, expires) = _jwt.Generate(user, roles);

            return new AuthResponseDto(
                user.Id, user.FullName, user.Email!, token, expires, roles.ToList());
        }
    }
}