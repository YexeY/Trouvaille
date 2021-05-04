using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Trouvaille_WebAPI.Models;

namespace AuthoDemoMVC.Models.Communication
{
    public class PostProductViewModel
    {
        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(16,2)")]
        public decimal Price { get; set; }

        [Required]
        public int InStock { get; set; }

        [Required]
        public int MinStock { get; set; }

        [Required]
        [Column(TypeName = "decimal(16,2)")]
        public decimal Tax { get; set; }

        //Manufacturer
        public Guid? ManufacturerId { get; set; }

        //Image
        public string? ImageTitle { get; set; }

        public byte[]? ImageData { get; set; }

        //Categories
        public ICollection<Guid> ProductCategoryIds { get; set; }

    }
}
