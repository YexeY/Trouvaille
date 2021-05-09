using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Models;
using AuthoDemoMVC.Models.Communication;
using Trouvaille.Models.Communication.Order;
using Trouvaille_WebAPI.Models;

namespace Trouvaille.Models.Communication.Customer
{
    public class GetCustomerViewModel
    {
        public string Id { get; set; } 

        [EmailAddress]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public AddressViewModel InvoiceAddress { get; set; }

        public AddressViewModel DeliveryAddress { get; set; }

        public ICollection<Guid> Orders { get; set; }

        private GetCustomerViewModel() { }

        public GetCustomerViewModel(ApplicationUser user)
        {
            Id = user.Id;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
            FirstName = user.FirstName;
            LastName = user.LastName;
            InvoiceAddress = new AddressViewModel(user.InvoiceAddress);
            DeliveryAddress = new AddressViewModel(user.DeliveryAddress);
            //Orders = user.Orders?.Select(o => new GetOrderViewModel(o)).ToList();
            Orders = user.Orders?.Select(o => o.OrderId).ToList();
        }
    }
}
