using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Trouvaille.Models.Communication.Category
{
    public class GetCategoryViewModel
    {
        [Key]
        public Guid CategoryId { get; set; }

        [Required]
        public string Name { get; set; }

        public int ProductCounter { get; set; }
#nullable enable
        public ICollection<Guid>? ProductIds { get; set; }
#nullable disable

        public GetCategoryViewModel(Trouvaille_WebAPI.Models.Category category)
        {
            CategoryId = category.CategoryId;
            Name = category.Name;
            ProductCounter = category.ProductCounter;
            ProductIds = category.Products?.Select(p => p.ProductId).ToList();
        }
    }
}
