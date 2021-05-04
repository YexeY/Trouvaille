using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Trouvaille_WebAPI.Models;

namespace AuthoDemoMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public Guid? DeliveryAddressId { get; set; }

        public Address? DeliveryAddress { get; set; }

        public Guid? InvoiceAddressId { get; set; }

        public Address? InvoiceAddress { get; set; }
    }
}
