using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Identity
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public string Key { get; set; } = null!;
        public int ExpiryMinutes { get; set; } = 60;
    }
}
