using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime;
using System.Security.Claims;
using System.Text;

namespace ECommerce.Infrastructure.Identity;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) Generate(ApplicationUser user, IList<string> roles);
}

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(IOptions<JwtSettings> options)
        => _settings = options.Value;

    public (string Token, DateTime ExpiresAtUtc) Generate(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("email", user.Email!),
            new("name", user.FullName),
            new("jti", Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}