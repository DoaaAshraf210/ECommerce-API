using ECommerce.Application.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Email
{
    public class LogEmailService : IEmailService
    {
        private readonly ILogger<LogEmailService> _logger;

        public LogEmailService(ILogger<LogEmailService> logger) => _logger = logger;

        public Task SendAsync(string to, string subject, string htmlBody)
        {
            _logger.LogInformation("DEV EMAIL to {To} | {Subject} | {Body}", to, subject, htmlBody);
            return Task.CompletedTask;
        }
    }
}