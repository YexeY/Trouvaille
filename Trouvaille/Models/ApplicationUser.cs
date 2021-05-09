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
#nullable enable
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public Guid? DeliveryAddressId { get; set; }

        public virtual Address? DeliveryAddress { get; set; }

        public Guid? InvoiceAddressId { get; set; }

        public virtual Address? InvoiceAddress { get; set; }

        public virtual ICollection<Order>? Orders { get; set; }
#nullable disable
    }
}
