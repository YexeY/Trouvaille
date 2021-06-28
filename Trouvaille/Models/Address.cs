using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthoDemoMVC.Models.Communication;

namespace Trouvaille_WebAPI.Models
{
    public class Address
    {
        [Key]
        public Guid AddressId { get; set; }

        public string Country { get; set; }

        public string State { get; set; }

        public string Street { get; set; }

        public int StreetNumber { get; set; }

        [Required]
        public Guid CityId { get; set; }

        public virtual City City { get; set; }

        public string ToString()
        {
            var address = new StringBuilder();
            address.AppendLine($"{Country} {State}");
            address.AppendLine($"{City.Name}  {City.PostalCode}");
            address.AppendLine($"{Street} {StreetNumber}");

            return address.ToString();
        }
    }
}
