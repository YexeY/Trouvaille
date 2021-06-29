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

        Task<bool> SendRestockEmailAsync(Manufacturer manufacturer, Product product);

        Task<bool> SendRestockOrderSelfEmailAsync(Product product, bool success);

        Task<bool> SendForgotPasswordEmailAsync(ApplicationUser user, string password);

        Task<bool> SendResetPasswordEmailAsync(ApplicationUser user);

        Task<bool> SendOrderChangedEmailAsync(ApplicationUser user, Order order);

        Task<bool> SendInvoiceEmailAsync(ApplicationUser user, Order order);
    }
}
