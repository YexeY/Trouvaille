using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentEmail.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Trouvaille.Services.MailService
{
    public class MailService : IMailService
    {
        private readonly IServiceProvider _serviceProvider;

        public MailService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async void SendEmailAsync(string toEmail, string subject, string content)
        {
            using var scope = _serviceProvider.CreateScope();
            var mailer = scope.ServiceProvider.GetRequiredService<IFluentEmail>();
            var email = mailer
                .To(toEmail)
                .Subject(subject)
                .Body(content);
            await email.SendAsync();
        }
    }
}
