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
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Trouvaille_WebAPI.Globals;
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
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string content)
        {
            if (content == null || subject == null || toEmail == null)
            {
                return false;
            };
            var email = Email
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
                .From(_configuration.GetSection("Gmail")["Sender"], _configuration.GetSection("Mail")["From"])
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
                .From(_configuration.GetSection("Gmail")["Sender"], _configuration.GetSection("Mail")["From"])
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
                .From(_configuration.GetSection("Gmail")["Sender"], _configuration.GetSection("Mail")["From"])
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

        public async Task<bool> SendForgotPasswordEmailAsync(ApplicationUser user, string password)
        {
            if (user == null)
            {
                return false;
            }

            var template = new StringBuilder();
            template.AppendLine($"Hello {user.FirstName},");
            template.AppendLine($"<p>Your new Password: {password}</p>");
            template.AppendLine("<p>With best regards</p>");
            template.AppendLine("<p>Trouvaille Online-Shop</p>");


            var email = Email
                .From(_configuration.GetSection("Gmail")["Sender"], _configuration.GetSection("Mail")["From"])
                .To(user.Email)
                .Subject("Forgot password")
                .UsingTemplate(template.ToString(), new { });
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

        public async Task<bool> SendResetPasswordEmailAsync(ApplicationUser user)
        {
            if (user == null)
            {
                return false;
            }

            var template = new StringBuilder();
            template.AppendLine($"Hello {user.FirstName},");
            template.AppendLine($"<p>You have successfully reset your password</p>");
            template.AppendLine("<p>With best regards</p>");
            template.AppendLine("<p>Trouvaille Online-Shop</p>");


            var email = Email
                .From(_configuration.GetSection("Gmail")["Sender"], _configuration.GetSection("Mail")["From"])
                .To(user.Email)
                .Subject("Password reset")
                .UsingTemplate(template.ToString(), new { });
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

        public async Task<bool> SendOrderChangedEmailAsync(ApplicationUser user, Order order)
        {
            if (user == null || order == null)
            {
                return false;
            }

            var template = new StringBuilder();
            template.AppendLine($"Hello {user.FirstName},");
            template.AppendLine($"<p>Your order with the id: {order.OrderId.ToString()}</p>");
            if (order.OrderState == Globals.OrderState.Storniert)
            {
                template.AppendLine($"<p>is now canceled</p>");
            }
            else if(order.OrderState == Globals.OrderState.Unterwegs)
            {
                template.AppendLine($"<p>is on the way!</p>");
            } else if (order.OrderState == Globals.OrderState.Zugestellt)
            {
                template.AppendLine($"<p>has been delivered, have fun with your product!</p>");
            }
            template.AppendLine("<p>With best regards</p>");
            template.AppendLine("<p>Trouvaille Online-Shop</p>");


            var email = Email
                .From(_configuration.GetSection("Gmail")["Sender"], _configuration.GetSection("Mail")["From"])
                .To(user.Email)
                .Subject("Order update")
                .UsingTemplate(template.ToString(), new { });
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

        public async Task<bool> SendInvoiceEmailAsync(ApplicationUser user, Order order)
        {
            if (user == null || order == null || user.InvoiceAddress  == null || user.InvoiceAddress.City == null
            || order.Products == null)
            {
                return false;
            }

            var template = new StringBuilder();
            template.AppendLine($"Hello {user.FirstName},");
            template.AppendLine($"<p>Here is your invoice with the ID: {order.Invoice_Id.ToString()}</p>");
            template.AppendLine($"<p>for your order: {order.OrderId.ToString()}</p>");
            template.AppendLine($"<p>Date: {DateTime.Now.ToString()}</p>");
            template.AppendLine($"<p>Service Provider: </p>");
            template.AppendLine($"<p>Name: {_configuration.GetSection("Invoice")["Name"]} </p>");
            template.AppendLine($"<p>Address: {_configuration.GetSection("Invoice")["Address"]} </p>");
            template.AppendLine($"<p>Address: {_configuration.GetSection("Invoice")["Address"]} </p>");
            template.AppendLine($"<p>Recipient: </p>");
            template.AppendLine($"<p>Name: {user.LastName},{user.FirstName} </p>");
            template.AppendLine($"<p>Address: {user.InvoiceAddress.ToString()}</p>");
            template.AppendLine($"<p>Iteams:</p>");
            foreach (var orderProduct in order.Products)
            {
                if (orderProduct.Product == null)
                {
                    return false;
                }
                template.AppendLine($"<p>{orderProduct.Cardinality} of {orderProduct.Product.Name}</p>");
            }
            template.AppendLine($"<p>Total: {order.TotalCost}</p>");
            template.AppendLine($"<p>Payment over: Payment in advance</p>");
            template.AppendLine($"<p>IBAN: {_configuration.GetSection("Invoice")["IBAN"]}</p>");
            template.AppendLine($"<p>BIC: {_configuration.GetSection("Invoice")["BIC"]}</p>");
            template.AppendLine($"<p>CustomerID: {user.Id.Substring(0, 27)}</p>");
            template.AppendLine($"<p>Usage: {order.Invoice_Id.ToString().Substring(0, 27)}</p>");
            template.AppendLine($"<p>Your order will be send  within 2-3 workday after reception  of payment </p>");

            template.AppendLine("<p>With best regards</p>");
            template.AppendLine("<p>Trouvaille Online-Shop</p>");


            var email = Email
                .From(_configuration.GetSection("Gmail")["Sender"], _configuration.GetSection("Mail")["From"])
                .To(user.Email)
                .Subject("Invoice")
                .UsingTemplate(template.ToString(), new { });
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
