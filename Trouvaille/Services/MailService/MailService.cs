using System;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using AuthoDemoMVC.Models;
using FluentEmail.Core;
using FluentEmail.Razor;
using FluentEmail.Smtp;
using Microsoft.Extensions.Logging;
using Trouvaille_WebAPI.Models;

namespace Trouvaille.Services.MailService
{
    public class MailService : IMailService
    {
        private readonly IFluentEmail _fluentEmail;

        public MailService(IFluentEmail fluentEmail)
        {
            _fluentEmail = fluentEmail;
            Email.DefaultSender = _fluentEmail.Sender;
            //Email.DefaultRenderer = new RazorRenderer();
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string content)
        {
            if (content == null || subject == null || toEmail == null)
            {
                return false;
            };
            var email = Email
                .From("trouvaille.customerservice@gmail.com", "Trouvaille Online-Shop")
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

        public async Task<bool> SendRegistrationConfirmationCustomerAsync(ApplicationUser customer)
        {
            if (customer == null)
            {
                return false;
            }

            var template = new StringBuilder();
            template.AppendLine($"Hello {customer.FirstName},");
            template.AppendLine("<p>Thank you for your registration at Trouvaille Online-Shop</p>");
            template.AppendLine("<p>Why don't you look around our assortment?</p>");
            template.AppendLine("<p>We're sure that there is something for you!</p>");
            template.AppendLine("<p>With best regards</p>");
            template.AppendLine("<p>Trouvaille Online-Shop</p>");


            var email = Email
                .From("trouvaille.customerservice@gmail.com", "Trouvaille Online-Shop")
                .To(customer.Email)
                .Subject("Registration")
                .UsingTemplate(template.ToString(), new {});
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

        public async Task<bool> SendOrderConfirmationEmailAsync(ApplicationUser customer, Order order)
        {
            if (customer == null || order == null)
            {
                return false;
            }

            var template = new StringBuilder();
            template.AppendLine($"<p>Hello {customer.FirstName},</p>");
            template.AppendLine("<p>Thank you for your Order!</p>");
            template.AppendLine($"<p>Invoice Number: {order.Invoice_Id}</p>");
            template.AppendLine($"<p>Total Cost: {order.TotalCost}</p>");
            template.AppendLine("<p>With the best Regard</p>");
            template.AppendLine("<p>your Trouvaille Online-Shop</p>");

            var email = Email
                .From("trouvaille.customerservice@gmail.com", "Trouvaille Online-Shop")
                .To(customer.Email)
                .Subject("Order confirmation")
                .UsingTemplate(template.ToString(),
                    new {FirstName = customer.FirstName, InvoiceId = order.Invoice_Id, TotalCost = order.TotalCost});
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
