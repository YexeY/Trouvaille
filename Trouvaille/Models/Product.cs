#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Models;

namespace Trouvaille_WebAPI.Models
{
    public class Product : IEquatable<Product>
    {
        [Key]
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(16,2)")]
        public decimal Price { get; set; }

        public Guid? ManufacturerId { get; set; }

        public virtual Manufacturer? Manufacturer { get; set; }

        public int InStock { get; set; }

        public int MinStock { get; set; }

        [Column(TypeName = "decimal(16,2)")]
        public decimal Tax { get; set; }

        public Guid? PictureId { get; set; }

        public virtual Picture? picture { get; set; }

        public virtual ICollection<Category>? ProductCategories { get; set; }

        public virtual ICollection<Rating>? Ratings { get; set; }

        public virtual ICollection<ApplicationUser>? Customer { get; set; }

        public bool Equals(Product? other)
        {
            return this.ProductId == other?.ProductId;
        }
    }
}
