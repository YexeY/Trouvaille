using System;
using System.Net;
using System.Net.Mail;
using FluentEmail.Core;
using FluentEmail.Smtp;

namespace Trouvaille.Services.MailService
{
    public class MailService : IMailService
    {
        private readonly IFluentEmail _fluentEmail;

        public MailService(IFluentEmail fluentEmail)
        {
            _fluentEmail = fluentEmail;
        }

        public async void SendEmailAsync(string toEmail, string subject, string content)
        {
            var sender = new SmtpSender(() => new SmtpClient("smtp.gmail.com")
            {
                UseDefaultCredentials = false,
                Port = 587,
                Credentials = new NetworkCredential("trouvaille.customerservice@gmail.com", "cuA3GJcMfmXeB5x"),
                EnableSsl = true
            });

            Email.DefaultSender = sender;

            var email = Email
                .From("trouvaille.customerservice@gmail.com", "Trouvaille Company")
                .To("yazici98@gmx.de")
                .Subject("Hello there")
                .Body("Wie gehts dir denn?");

            try
            {
                await email.SendAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
