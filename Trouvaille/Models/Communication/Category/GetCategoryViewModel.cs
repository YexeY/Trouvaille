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

        public ICollection<Guid>? ProductIds { get; set; }
    }
}
