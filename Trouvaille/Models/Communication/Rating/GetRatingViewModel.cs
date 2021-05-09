using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Models;

namespace Trouvaille.Models.Communication.Rating
{
    public class GetRatingViewModel
    {
        [Key]
        public Guid RatingId { get; set; }

        public string CustomerId { get; set; }

        public ApplicationUser Customer{ get; set; }

        public Guid ProductId { get; set; }

        public virtual Trouvaille_WebAPI.Models.Product? Product { get; set; }

        [Column(TypeName = "decimal(16,1)")]
        public decimal StarCount { get; set; }

#nullable enable
        public string? Title { get; set; }

        public string? Description { get; set; }
#nullable disable

        public GetRatingViewModel(Trouvaille_WebAPI.Models.Rating rating)
        {
            RatingId = rating.RatingId;
            CustomerId = rating.CustomerId;
            Customer = rating.Customer;
            ProductId = rating.ProductId;
            Product = rating.Product;
            StarCount = rating.StarCount;
            Title = rating.Title;
            Description = rating.Description;
        }
    }
}
