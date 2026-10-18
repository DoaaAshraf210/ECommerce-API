using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Identity
{
    public class ApplicationUser: IdentityUser<Guid>
    {
        public string FullName { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}
