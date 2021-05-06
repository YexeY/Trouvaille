using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Trouvaille_WebAPI.Models;

namespace Trouvaille.Models.Communication.Product
{
    public class GetProductViewModel
    {
        [Key]
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(16,2)")]
        public decimal Price { get; set; }

        public Guid? ManufacturerId { get; set; }

        public int InStock { get; set; }


        [Column(TypeName = "decimal(16,2)")]
        public decimal Tax { get; set; }

        public Guid? PictureId { get; set; }

        public virtual Picture? picture { get; set; }

        public virtual List<Guid>? ProductCategories { get; set; }
    }
}
