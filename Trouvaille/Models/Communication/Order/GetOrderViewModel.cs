using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Models.Communication;

namespace Trouvaille.Models.Communication.Order
{
    public class GetOrderViewModel
    {
        [Key]
        public Guid OrderId { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateTime Date { get; set; }

        public Trouvaille_WebAPI.Globals.Globals.PaymentMethod PaymentMethod { get; set; }

        public Trouvaille_WebAPI.Globals.Globals.Shipmentmethod ShipmentMethod { get; set; }

        public Trouvaille_WebAPI.Globals.Globals.OrderState OrderState { get; set; }

        public AddressViewModel DeliveryAddress { get; set; }

        public AddressViewModel InvoiceAddress { get; set; }

        [Required]
        public ICollection<PostOrderProductViewModel> Products { get; set; }

        public GetOrderViewModel(Trouvaille_WebAPI.Models.Order order)
        {
            OrderId = order.OrderId;
            Date = order.Date;
            PaymentMethod = order.PaymentMethod;
            ShipmentMethod = order.ShipmentMethod;
            OrderState = order.OrderState;
            DeliveryAddress = new AddressViewModel(order.DeliveryAddress);
            InvoiceAddress = new AddressViewModel(order.InvoiceAddress);
            ICollection<PostOrderProductViewModel> products = new List<PostOrderProductViewModel>();
            foreach (var orderProduct in order.Products)
            {
                products.Add(new PostOrderProductViewModel(orderProduct));
            }
            Products = products;
        }
    }
}
