using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Trouvaille.Models.Communication.Manufacturer
{
    public class GetManufacturerViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? CatalogId { get; set; }
    }
}
