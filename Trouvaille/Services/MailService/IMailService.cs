using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trouvaille.Services.MailService
{
    interface IMailService
    {
        void SendEmailAsync(string toEmail, string subject, string content);
    }
}
