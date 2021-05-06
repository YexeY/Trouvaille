using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Trouvaille_WebAPI.Models;

namespace AuthoDemoMVC.Models
{
    public class OrderProduct
    {
        [Key]
        public Guid OderProductId { get; set; }

        [Required]
        public Guid OrderId { get; set; }
        [Required]
        public virtual Order Order { get; set; }
        [Required]
        public Guid ProductId { get; set; }
        [Required]

        public virtual Product Product { get; set; }
        [Required]
        public int Cardinality { get; set; }
    }
}
