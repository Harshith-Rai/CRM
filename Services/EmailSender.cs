using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
// This creates a 'logs.txt' file in your project folder


namespace CRM.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string html)
        {
            try
            {
                var msg = new MimeMessage();
                var fromEmail = _config["Smtp:From"];
                msg.From.Add(new MailboxAddress("CRM System", fromEmail));
                msg.To.Add(MailboxAddress.Parse(to));
                msg.Subject = subject;
                msg.Body = new TextPart("html") { Text = html };

                string logFileName = $"smtp_log_{DateTime.Now.Ticks}.txt";
                using var client = new MailKit.Net.Smtp.SmtpClient(new ProtocolLogger(logFileName));

                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.CheckCertificateRevocation = false;

                var host = _config["Smtp:Host"];
                var port = int.Parse(_config["Smtp:Port"] ?? "587");
                var user = _config["Smtp:User"];
                var pass = _config["Smtp:Pass"];

                SecureSocketOptions security = port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                await client.ConnectAsync(host, port, security);

                if (!string.IsNullOrEmpty(user))
                {
                    await client.AuthenticateAsync(user, pass);
                }

                await client.SendAsync(msg);
                await client.DisconnectAsync(true);

                Debug.WriteLine(">>> Email sent successfully to " + to);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(">>> EMAIL ERROR: " + ex.Message);
                if (ex.InnerException != null)
                    Debug.WriteLine(">>> INNER ERROR: " + ex.InnerException.Message);

                throw;
            }
        }
    }
}