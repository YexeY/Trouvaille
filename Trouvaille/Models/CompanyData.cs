using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Trouvaille_WebAPI.Models
{
    public class CompanyData
    {
        [Key]
        public Guid CompanyDataId { get; set; }

        public int TaxNumber { get; set; }

        public string Name { get; set; }

        public string IBAN { get; set; }

        public virtual Address Residency { get; set; }
    }
}
