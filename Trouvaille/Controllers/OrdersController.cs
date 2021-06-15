using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthoDemoMVC.Data;
using AuthoDemoMVC.Models;
using AuthoDemoMVC.Models.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Trouvaille.Models.Communication.Order;
using Trouvaille.Services.MailService;
using Trouvaille_WebAPI.Models;

namespace AuthoDemoMVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMailService _mailService;

        public OrdersController(ApplicationDbContext context, IMailService mailService)
        {
            _context = context;
            _mailService = mailService;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetOrderViewModel>>> GetOrder()
        {
            var orders = await _context.Order
                .Include(o => o.DeliveryAddress)
                .Include(o => o.InvoiceAddress)
                .Include(o => o.DeliveryAddress.City)
                .Include(o => o.InvoiceAddress.City)
                .Include(o => o.Products)
                .ToListAsync();
            var getOrderViewModels = orders.Select(p => new GetOrderViewModel(p)).ToList();
            return getOrderViewModels;
        }

        // GET: api/Orders/6/11
        [HttpGet("{from}/{to}")]
        public async Task<ActionResult<IEnumerable<GetOrderViewModel>>> GetOrderFromTo(int from, int to)
        {
            var orders = await _context.Order
                .Skip(from)
                .Take(to - from)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.InvoiceAddress)
                .Include(o => o.DeliveryAddress.City)
                .Include(o => o.InvoiceAddress.City)
                .Include(o => o.Products)
                .ToListAsync();
            var getOrderViewModels = orders.Select(p => new GetOrderViewModel(p)).ToList();
            return getOrderViewModels;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetOrderViewModel>>> GetOrdersFromTo()
        {
            var orders = await _context.Order
                .Include(o => o.DeliveryAddress)
                .Include(o => o.InvoiceAddress)
                .Include(o => o.DeliveryAddress.City)
                .Include(o => o.InvoiceAddress.City)
                .Include(o => o.Products)
                .ToListAsync();
            var getOrderViewModels = orders.Select(p => new GetOrderViewModel(p)).ToList();
            return getOrderViewModels;
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetOrderViewModel>> GetOrder(Guid id)
        {
            var order = await _context.Order
                .Include(o => o.DeliveryAddress)
                .Include(o => o.InvoiceAddress)
                .Include(o => o.DeliveryAddress.City)
                .Include(o => o.InvoiceAddress.City)
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            var getOrderViewModel = new GetOrderViewModel(order);
            return getOrderViewModel;
        }

        /**
        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(Guid id, Order order)
        {
            if (id != order.OrderId)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        **/

        // POST: api/Orders
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<GetOrderViewModel>> PostOrder(PostOrderViewModel model)
        {
            //VERIFY USER ROLE
            //-------------------------------------------
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.Products).FirstOrDefaultAsync(u => u.Id == userId.Value);
            //TODO Verify Role
            /**
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var value = identity.FindFirst("Role").Value;
                if (value != "Customer")
                {
                    return Unauthorized("Not Authorized");
                }
            }
            **/
            //-------------------------------------------
            //Extract Adresses
            //-------------------------------------------
            var invoiceAddress = AddressViewModel.GetAddress(model.InvoiceAddress);
            var deliveryAddress = model.DeliveryAddress == null
                ? invoiceAddress
                : AddressViewModel.GetAddress(model.DeliveryAddress);
            //-------------------------------------------


            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                Invoice_Id = Guid.NewGuid(),
                Date = DateTime.Now,
                PaymentMethod = model.PaymentMethod,
                ShipmentMethod = model.ShipmentMethod,
                OrderState = model.OrderState,
                InvoiceAddress = invoiceAddress,
                DeliveryAddress = deliveryAddress,
                CustomerId = user?.Id
            };

            //Extract Products and add to Order
            //------------------------------------------
            ICollection<OrderProduct> orderProducts = new List<OrderProduct>();
            ICollection<Product> products = new List<Product>();
            foreach (var VARIABLE in model.Products)
            {
                var product = _context.Product.Find(VARIABLE.ProductId);

                if (product == null)
                {    continue;}

                var cardinality = VARIABLE.Cardinality;

                if (product.InStock - cardinality < product.MinStock)
                {
                    return BadRequest("Don't have that many in stock");
                }

                if (product.InStock - cardinality < product.MinStock)
                {
                    //TODO sth
                    Console.WriteLine("Dont have that many in stock");
                }

                product.InStock -= cardinality;
                _context.Entry(product).State = EntityState.Modified;

                //TODO Add Product to USerProductList if not already contained

                if (user.Products == null)
                {
                    user.Products = new List<Product> {product};
                } 
                else if (user.Products.Any(p => p.ProductId == product.ProductId) != true)
                {
                    user.Products.Add(product);
                }

                //_context.Entry(user).State = EntityState.Modified;
                var orderProduct = new OrderProduct
                {
                    Product = product,
                    Cardinality = cardinality,
                    OderProductId = Guid.NewGuid(),
                    ProductId = product.ProductId,
                    Order = order,
                    OrderId = order.OrderId
                };
                orderProducts.Add(orderProduct);
            }
            order.Products = orderProducts;
            //Calculate total Cost
            //TODO what about tax?
            ICollection<decimal> costList = order.Products.Select(i => i.Cardinality * i.Product.Price).ToList();
            foreach (var @decimal in costList)
            {
                order.TotalCost += @decimal;
            }

            //------------------------------------------
            await _context.Order.AddAsync(order);
            await _context.SaveChangesAsync();

            _mailService.SendOrderConfirmationEmailAsync(user, order);

            var getOrderViewModel = new GetOrderViewModel(order);
            return CreatedAtAction("GetOrder", new { id = order.OrderId }, getOrderViewModel);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Order.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(Guid id)
        {
            return _context.Order.Any(e => e.OrderId == id);
        }

        // POST: api/Orders/SearchQuery
        [HttpPost]
        [Route("{from}/{to}")]
        public async Task<ActionResult<GetOrderViewModel>> SearchQueryOrder(int from, int to, Guid customerId,
            DateTime fromDateTime, DateTime toDateTime,  int orderState)
        {

            return null;
        }
    }
}
