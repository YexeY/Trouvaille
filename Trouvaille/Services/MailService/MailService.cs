using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Smtp;
using Microsoft.Extensions.Logging;

namespace Trouvaille.Services.MailService
{
    public class MailService : IMailService
    {
        private readonly IFluentEmail _fluentEmail;

        public MailService(IFluentEmail fluentEmail)
        {
            _fluentEmail = fluentEmail;
            Email.DefaultSender = _fluentEmail.Sender;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string content)
        {
            if (content == null || subject == null || toEmail == null)
            {
                return false;
            };
            var email = Email
                .From("trouvaille.customerservice@gmail.com", "Trouvaille Company")
                .To(toEmail)
                .Subject(subject)
                .Body(content);
            try
            {
                await email.SendAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return true;
        }
    }
}
