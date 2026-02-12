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

        //public async Task SendEmailAsync(string to, string subject, string html)
        //{
        //    try
        //    {
        //        var msg = new MimeMessage();
        //        var fromEmail = _config["Smtp:From"];
        //        msg.From.Add(new MailboxAddress("CRM System", fromEmail));
        //        msg.To.Add(MailboxAddress.Parse(to));
        //        msg.Subject = subject;
        //        msg.Body = new TextPart("html") { Text = html };

        //        using var client = new SmtpClient();

        //        // Bypass SSL certificate validation for local development
        //        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

        //        var host = _config["Smtp:Host"];
        //        var port = int.Parse(_config["Smtp:Port"] ?? "587");

        //        // 587 REQUIRES StartTls
        //        var user = _config["Smtp:User"];
        //        var pass = _config["Smtp:Pass"];


        //        // ADD THIS BLOCK HERE:
        //        SecureSocketOptions security;
        //        if (port == 465)
        //        {
        //            security = SecureSocketOptions.SslOnConnect; // Implicit SSL
        //        }
        //        else
        //        {
        //            security = SecureSocketOptions.StartTls;     // Explicit SSL/TLS for 587
        //        }

        //        // Pass the 'security' variable into the connect method
        //        await client.ConnectAsync(host, port, security);

        //        // ... existing AuthenticateAsync code below ...
        //        // Use SecureSocketOptions.Auto - this fixes the Handshake Exception
        //        // because it negotiates the correct protocol based on the port


        //        if (!string.IsNullOrEmpty(user))
        //        {
        //            await client.AuthenticateAsync(user, pass);
        //        }

        //        await client.SendAsync(msg);
        //        Debug.WriteLine(">>> Email sent successfully to " + to);

        //        await client.DisconnectAsync(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(">>> EMAIL ERROR: " + ex.Message);
        //        if (ex.InnerException != null)
        //        {
        //            Debug.WriteLine(">>> INNER ERROR: " + ex.InnerException.Message);
        //        }
        //        throw;
        //    }
        //}
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
                using var client = new SmtpClient(new ProtocolLogger(logFileName));

                // CRITICAL: This must be set before any connection attempt
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // This line forces the client to use the most compatible SSL/TLS versions
                client.CheckCertificateRevocation = false;

                var host = _config["Smtp:Host"];
                var port = int.Parse(_config["Smtp:Port"] ?? "587");
                var user = _config["Smtp:User"];
                var pass = _config["Smtp:Pass"];

                // If your office network blocks port 587, try using port 465 
                // with SslOnConnect in your appsettings.json
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
                // If it still fails, check the InnerException for specific TLS details
                if (ex.InnerException != null)
                    Debug.WriteLine(">>> INNER ERROR: " + ex.InnerException.Message);

                throw;
            }
        }
    }
}