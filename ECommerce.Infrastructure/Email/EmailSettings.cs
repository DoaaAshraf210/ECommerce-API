using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Email
{
    public class EmailSettings
    {

        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromAddress { get; set; } = "no-reply@ecommerce.local";
        public string FromName { get; set; } = "ECommerce";


    }
}
