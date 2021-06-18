using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Trouvaille.Models.Communication.Address
{
    public class PutAddressViewModel
    {
        [StringLength(50)]
        public string? Country { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(50)]
        public string? Street { get; set; }

        public int? StreetNumber { get; set; }

        public int? PostalCode { get; set; }

        [StringLength(50)]
        public string? CityName { get; set; }
    }
}
