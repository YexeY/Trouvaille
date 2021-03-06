using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Trouvaille_WebAPI.Models
{
    public class Manufacturer
    {
        [Key]
        public Guid ManufacturerId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? CatalogId { get; set; }
    }
}
