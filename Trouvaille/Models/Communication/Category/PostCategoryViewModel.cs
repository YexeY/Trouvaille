using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Trouvaille.Models.Communication.Category
{
    public class PostCategoryViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}
