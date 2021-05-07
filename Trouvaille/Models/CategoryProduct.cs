using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Trouvaille_WebAPI.Models;

namespace Trouvaille.Models
{
    public class CategoryProduct
    {
        public Guid ProductCategoriesCategoryId { get; set; }
        public Category ProductCategoriesCategory { get; set; }

        public Guid ProductsProductId { get; set; }
        public Product ProductsProduct { get; set; }
    }
}
