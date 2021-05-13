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

        public int? InStock { get; set; }


        [Column(TypeName = "decimal(16,2)")]
        public decimal? Tax { get; set; }

        public Guid? PictureId { get; set; }

        public virtual Picture? Picture { get; set; }

        public virtual List<Guid>? ProductCategories { get; set; }

        public virtual ICollection<Guid>? Ratings { get; set; }

        public decimal? AverageRating { get; set; }

        //TODO RATING DURCHSCHNITT
        //TODO RATING GUIDS

        public GetProductViewModel(Trouvaille_WebAPI.Models.Product product)
        {  
            ProductId = product.ProductId;
            Description = product.Description;
            ManufacturerId = product.ManufacturerId;
            InStock = product.InStock;
            Picture = product.picture;
            Name = product.Name;
            PictureId = product.PictureId;
            Price = product.Price;
            Tax = product.Tax;
            ProductCategories = product.ProductCategories?.Select(p => p.CategoryId).ToList();
            Ratings = product.Ratings?.Select(r => r.RatingId).ToList();
            AverageRating = product.AverageRating;
        }
    }
}
