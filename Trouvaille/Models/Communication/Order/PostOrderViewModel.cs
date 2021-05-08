using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Trouvaille.Models.Communication.Order;
using Trouvaille_WebAPI.Models;

namespace AuthoDemoMVC.Models.Communication
{
    public class PostOrderViewModel
    {
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateTime Date { get; set; }

        public Trouvaille_WebAPI.Globals.Globals.PaymentMethod PaymentMethod { get; set; }

        public Trouvaille_WebAPI.Globals.Globals.Shipmentmethod ShipmentMethod { get; set; }

        public Trouvaille_WebAPI.Globals.Globals.OrderState OrderState { get; set; }

        public AddressViewModel? DeliveryAddress { get; set; }

        public AddressViewModel InvoiceAddress { get; set; }

        [Required]
        public ICollection<PostOrderProductViewModel> Products { get; set; }
    }
}
