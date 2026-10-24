using ECommerce.Application.Services;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;
namespace ECommerce.Infrastructure.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _s;

        public SmtpEmailService(IOptions<EmailSettings> options) => _s = options.Value;

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_s.FromName, _s.FromAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_s.Host, _s.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_s.Username, _s.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}