using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Models;

namespace Trouvaille.Models.Communication.Rating
{
    public class PostRatingViewModel
    {
        public Guid ProductId { get; set; }

        [Column(TypeName = "decimal(16,1)")]
        public decimal StarCount { get; set; }

#nullable enable
        public string? Title { get; set; }

        public string? Description { get; set; }
#nullable disable
    }
}
