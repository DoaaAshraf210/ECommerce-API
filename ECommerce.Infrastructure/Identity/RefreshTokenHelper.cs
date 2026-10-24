using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ECommerce.Infrastructure.Identity
{
    public static class RefreshTokenHelper
    {

        public static string Generate()
            => Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        public static string Hash(string token)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
