using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Models;
using Trouvaille_WebAPI.Models;

namespace Trouvaille.Services.MailService
{
    public interface IMailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string content);

        Task<bool> SendRegistrationConfirmationCustomerAsync(ApplicationUser customer);

        Task<bool> SendOrderConfirmationEmailAsync(ApplicationUser customer, Order order);
    }
}
