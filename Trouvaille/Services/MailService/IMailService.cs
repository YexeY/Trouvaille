using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trouvaille.Services.MailService
{
    public interface IMailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string content);
    }
}
