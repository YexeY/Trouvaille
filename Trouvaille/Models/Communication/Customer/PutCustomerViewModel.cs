using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Models.Communication;
using Trouvaille.Models.Communication.Address;

namespace Trouvaille.Models.Communication.Customer
{
    public class PutCustomerViewModel
    {
        [EmailAddress]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public Boolean? IsDisabled { get; set; }

        public PutAddressViewModel? InvoiceAddress { get; set; }

        public PutAddressViewModel? DeliveryAddress { get; set; }
    }
}
