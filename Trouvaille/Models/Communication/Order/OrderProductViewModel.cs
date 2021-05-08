using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Trouvaille_WebAPI.Models;

namespace AuthoDemoMVC.Models.Communication
{
    public class OrderProductViewModel 
    {
        public Guid ProductId { get; set; }
        [Required]

        public int Cardinality { get; set; }

        public OrderProductViewModel(Guid productId, int cardinality)
        {
            ProductId = productId;
            Cardinality = cardinality;
        }

        public OrderProductViewModel(OrderProduct orderProduct)
        {
            ProductId = orderProduct.ProductId;
            Cardinality = orderProduct.Cardinality;
        }

        public OrderProductViewModel() { }
    }
}
