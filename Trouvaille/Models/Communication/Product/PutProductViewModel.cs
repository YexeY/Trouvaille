using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Trouvaille.Models.Communication.Product
{
    public class PutProductViewModel
    { 
        public string? Name { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(16,2)")]
        public decimal? Price { get; set; }

        public int? InStock { get; set; }

        public int? MinStock { get; set; }

        [Column(TypeName = "decimal(16,2)")]
        public decimal? Tax { get; set; }

        public bool? IsDisabled { get; set; }

#nullable enable
        //Manufacturer
        public string? ManufacturerEmail { get; set; }

        public string? ManufacturerCatalogId { get; set; }
        //

        public string? ImageTitle { get; set; }

        public byte[]? ImageData { get; set; }
    }
}
