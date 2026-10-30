using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace ECommerce.Infrastructure.Identity
{
    public static class RefreshTokenHelper
    {

        public static string Generate()
            => Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        public static string Hash(string token)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        public static Task<int> RevokeAllAsync(ApplicationDbContext db, Guid userId)
        {
            var now = DateTime.UtcNow;
            return db.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAtUtc, now));
        }
    }
}
