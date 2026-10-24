using ECommerce.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RefreshAsync(RefreshTokenDto dto);
        Task LogoutAsync(RefreshTokenDto dto);
    }
}
