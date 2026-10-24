using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody);


    }
}
