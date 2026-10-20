using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.ValueObjects
{
    public class ShippingAddress
    {
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? PostalCode { get; set; }
    }
}
