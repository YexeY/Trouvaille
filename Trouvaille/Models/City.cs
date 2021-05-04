using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Trouvaille_WebAPI.Models
{
    public class City
    {
        [Key]
        public Guid CityId { get; set; }

        public int PostalCode { get; set; }

        public string Name { get; set; }
    }
}
