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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Trouvaille_WebAPI.Models;

namespace Trouvaille.Services.MailService
{
    public class MailService : IMailService
    {
        private readonly IFluentEmail _fluentEmail;
        private readonly IConfiguration _configuration;
        public MailService(IFluentEmail fluentEmail, IConfiguration configuration)
        {
            _fluentEmail = fluentEmail;
            Email.DefaultSender = _fluentEmail.Sender;
            _configuration = configuration;
            //Email.DefaultRenderer = new RazorRenderer();
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string content)
        {
            if (content == null || subject == null || toEmail == null)
            {
                return false;
            };
            var email = Email
                //.From("trouvaille.customerservice@gmail.com", "Trouvaille Online-Shop")
                .From(_configuration.GetSection("Gmail")["Sender"], _configuration.GetSection("Mail")["From"])
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
                //.From("trouvaille.customerservice@gmail.com", "Trouvaille Online-Shop")
                .From(_configuration.GetSection("Gmail")["Sender"], _configuration.GetSection("Mail")["From"])
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

        public async Task<bool> SendRestockEmailAsync(Manufacturer manufacturer, Product product)
        {
            if (manufacturer == null || product == null || manufacturer.Email == null)
            {
                return false;
            }

            var template = new StringBuilder();
            template.AppendLine($"<p>Hello,</p>");
            template.AppendLine($"<p>We would like to Order {(product.MinStock + (int)(product.MinStock * 0.5))} of {product.Name}</p>");
            if (!string.IsNullOrEmpty(manufacturer.CatalogId))
            {
                template.AppendLine($"<p>Catalog Number: {manufacturer.CatalogId}</p>");
            }
            template.AppendLine("<p>With the best Regard</p>");
            template.AppendLine("<p>Trouvaille Online-Shop</p>");
            //TODO: Address

            var email = Email
                .From("trouvaille.customerservice@gmail.com", "Trouvaille Online-Shop")
                .To(manufacturer.Email)
                .Subject("Restock Order")
                .UsingTemplate(template.ToString(), new { Minstock = product.MinStock, ProductName = product.Name});
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

        public async Task<bool> SendRestockOrderSelfEmailAsync(Product product)
        {
            var template = new StringBuilder();
            template.AppendLine($"<p>Hello,</p>");
            template.AppendLine($"<p>We need to Order {(product.MinStock + (int)(product.MinStock * 0.5))} of {product.Name}</p>");
            template.AppendLine($"<p>ProductId: {product.ProductId.ToString()}</p>");
            template.AppendLine("<p>With the best Regard</p>");
            template.AppendLine("<p>Trouvaille Online-Shop</p>");

            var email = Email
                .From("trouvaille.customerservice@gmail.com", "Trouvaille Online-Shop")
                .To(_configuration.GetSection("Email")["RestockEmail"])
                .Subject("Restock Order")
                .UsingTemplate(template.ToString(), new { Minstock = product.MinStock, ProductName = product.Name });
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
