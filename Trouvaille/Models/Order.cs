using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using AuthoDemoMVC.Models;

namespace Trouvaille_WebAPI.Models
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }

        public Guid Invoice_Id { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateTime Date { get; set; }

        public Globals.Globals.PaymentMethod PaymentMethod { get; set; }

        public Globals.Globals.Shipmentmethod ShipmentMethod { get; set; }

        public Globals.Globals.OrderState OrderState { get; set; }

        public virtual Address DeliveryAddress { get; set; }

        public virtual Address InvoiceAddress { get; set; }

        public string CustomerId { get; set; }

        public virtual ApplicationUser Customer { get; set; }

        [Required]
        public virtual ICollection<OrderProduct> Products { get; set; }
    }
}
