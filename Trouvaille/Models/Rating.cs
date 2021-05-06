using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Models;

namespace Trouvaille_WebAPI.Models
{
    public class Rating
    {
        [Key]
        public Guid RatingId { get; set; }

        public Guid CustomerId { get; set; }

        public virtual ApplicationUser Customer { get; set; }

        public Guid ProductId { get; set; }

        public virtual Product Product { get; set; }

        [Column(TypeName = "decimal(16,1)")]
        public decimal Starcount { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }
    }
}
