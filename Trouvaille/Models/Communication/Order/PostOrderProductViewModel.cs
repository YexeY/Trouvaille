using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Models;

namespace Trouvaille.Models.Communication.Order
{
    public class PostOrderProductViewModel
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public int Cardinality { get; set; }

        public PostOrderProductViewModel(OrderProduct orderProduct)
        {
            ProductId = orderProduct.ProductId;
            Cardinality = orderProduct.Cardinality;
        }

        public PostOrderProductViewModel() { }
    }
}
