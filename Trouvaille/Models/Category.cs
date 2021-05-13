using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Trouvaille_WebAPI.Models
{
    public class Category : IEquatable<Category>
    {
        [Key]
        public Guid CategoryId { get; set; }

        [Required]
        public string Name { get; set; }

        public int ProductCounter { get; set; }

        public virtual  ICollection<Product> Products { get; set; }

        public bool Equals(Category? other)
        {
            return other != null && this.CategoryId.Equals(other.CategoryId);
        }
    }
}
