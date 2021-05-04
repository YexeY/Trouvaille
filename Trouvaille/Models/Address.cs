using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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

        public City City { get; set; }
    }
}
