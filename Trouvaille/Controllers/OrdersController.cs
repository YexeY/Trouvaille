using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AuthoDemoMVC.Data;
using AuthoDemoMVC.Models;
using AuthoDemoMVC.Models.Communication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trouvaille.Models.Communication.Order;
using Trouvaille.Services.MailService;
using Trouvaille_WebAPI.Globals;
using Trouvaille_WebAPI.Models;

namespace Trouvaille.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
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
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public async Task<ActionResult<ICollection<GetOrderViewModel>>> GetOrder()
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
        [Microsoft.AspNetCore.Mvc.HttpGet("{from}/{to}")]
        public async Task<ActionResult<ICollection<GetOrderViewModel>>> GetOrderFromTo(int from, int to)
        {
            if (to <= from)
            {
                return BadRequest("to must be greater then from");
            }

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

        // GET: api/Orders/5
        [Microsoft.AspNetCore.Mvc.HttpGet("{id}")]
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

        // POST: api/Orders
        //[Microsoft.AspNetCore.Authorization.Authorize]
        [Microsoft.AspNetCore.Mvc.HttpPost]
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
                var product = await _context.Product.FindAsync(VARIABLE.ProductId);

                if (product == null)
                {
                    continue;
                }

                var cardinality = VARIABLE.Cardinality;

                if (product.InStock - cardinality < product.MinStock)
                {
                    return BadRequest("Don't have that many in stock");
                }

                if (product.IsDisabled)
                {
                    return BadRequest($"Product with ID: {product.ProductId} is Disabled");
                }

                if (product.InStock - cardinality < product.MinStock)
                {
                    SendRestockEmailAsync(product.ManufacturerId, product);
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
            try
            {
                await _context.Order.AddAsync(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            _mailService.SendOrderConfirmationEmailAsync(user, order);
            sendInvoiceEmail(order, user.Id);

            var getOrderViewModel = new GetOrderViewModel(order);
            return CreatedAtAction("GetOrder", new { id = order.OrderId }, getOrderViewModel);
        }

        // DELETE: api/Orders/5
        [Microsoft.AspNetCore.Mvc.HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Order.Remove(order);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        private bool OrderExists(Guid id)
        {
            return _context.Order.Any(e => e.OrderId == id);
        }

        // POST: api/Orders/SearchQuery
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("{from}/{to}")]
        public async Task<ActionResult<ICollection<GetOrderViewModel>>> SearchQueryOrder(int from, int to, Guid? customerId = null,
            string? fromDateTime = null, string? toDateTime = null,  int? orderState = null, string orderBy = "Date", bool asc = true)
        {
            if (to <= from)
            {
                return BadRequest("to must be greater then from");
            }
            string dateFormats = "yyyy-MM-dd";
            Boolean and = false;
            Boolean where = false;
            StringBuilder query = new StringBuilder();
            query.AppendLine("  select * from [Order] O");
            if (customerId != null || fromDateTime != null || toDateTime != null || orderState != null)
            {
                query.AppendLine(" where (");
                where = true;
            }
            if (customerId != null)
            {
                query.AppendLine($"  CustomerId = '{customerId.ToString()}'");
                and = true;
            }

            if (fromDateTime != null)
            {
                if (!IsValidDate(fromDateTime, dateFormats))
                {
                    return BadRequest("Bad DateTime format");
                }
                query.AppendLine(and ? " and " : "");
                query.AppendLine($"  Date >= '{fromDateTime}'");
                and = true;
            }
            if (toDateTime != null)
            {
                if (!IsValidDate(toDateTime, dateFormats))
                {
                    return BadRequest("Bad DateTime format");
                }
                query.AppendLine(and ? " and " : "");
                query.AppendLine($"  Date >= '{toDateTime}'");
                and = true;
            }
            if (orderState != null)
            {
                query.AppendLine(and ? " and " : "");
                query.AppendLine($"  OrderState = {orderState.Value}");
            }
            query.AppendLine(where ? " ) " : "");

            var orders = new List<Order>();
            if (asc)
            {
                orders = await _context.Order.FromSqlRaw(query.ToString())
                    .Include(o => o.DeliveryAddress)
                    .Include(o => o.InvoiceAddress)
                    .Include(o => o.DeliveryAddress.City)
                    .Include(o => o.InvoiceAddress.City)
                    .Include(o => o.Products)
                    .OrderBy(o => o.Date)
                    .Skip(from)
                    .Take(to - from)
                    .ToListAsync();
            }
            else
            {
                orders = await _context.Order.FromSqlRaw(query.ToString())
                    .Include(o => o.DeliveryAddress)
                    .Include(o => o.InvoiceAddress)
                    .Include(o => o.DeliveryAddress.City)
                    .Include(o => o.InvoiceAddress.City)
                    .Include(o => o.Products)
                    .OrderByDescending(o => o.Date)
                    .Skip(from)
                    .Take(to - from)
                    .ToListAsync();
            }

            var getOrderViewModel = orders.Select(o => new GetOrderViewModel(o)).ToList();

            return Ok(getOrderViewModel);
        }

        // GET: api/Orders/Count
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("Count")]
        public async Task<ActionResult<int>> GetNumberOfOrders()
        {
            var count = await _context.Order.CountAsync();
            return Ok(count);
        }

        //Post: api/Orders/History/0/5
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("History/{from}/{to}")]
        //[Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult<ICollection<GetOrderViewModel>>> GetHistory(int from, int to
            , Guid? customerId = null)
        {
            if (to <= from)
            {
                return BadRequest("to must be greater then from");
            }
            string id;
            if (customerId == null)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent("Customer Id Could Not Be Found"),
                        ReasonPhrase = "Unclear why this could be the Case, contact Admin"
                    };

                    throw new HttpResponseException(response);
                }
                id = userId.Value;
            }   
            else
            {
                id = customerId.ToString();
            }

            var query = new StringBuilder();
            query.AppendLine($" select * from [Order] O");
            query.AppendLine($" where O.CustomerId = '{id}'");

            var orders =  _context.Order.FromSqlRaw(query.ToString())
                .Include(o => o.DeliveryAddress)
                .Include(o => o.InvoiceAddress)
                .Include(o => o.DeliveryAddress.City)
                .Include(o => o.InvoiceAddress.City)
                .Include(o => o.Products)
                .OrderBy(o => o.Date)
                .Skip(from)
                .Take(to -from)
                .ToList();

            //TODO: is this necessary?
            if (!orders.Any())
            {
                return NotFound("No Orders were found");
            }


            var getOrderViewModels = orders.Select(o => new GetOrderViewModel(o)).ToList();

            return Ok(getOrderViewModels);
        }

        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("History/Count")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult<int>> GetHistoryCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Customer Id Could Not Be Found"),
                    ReasonPhrase = "Unclear why this could be the Case, contact Admin"
                };

                throw new HttpResponseException(response);
            }
            var id = userId.Value;
            var count = await _context.Order.Where(o => o.CustomerId == id).CountAsync();
            return Ok(count);
        }


        // PUT: api/Orders/5
        [Microsoft.AspNetCore.Mvc.HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(Guid id, Globals.OrderState orderState)
        {

            var order = await _context.Order.FindAsync(id);
            order.OrderState = orderState;
            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                var user = await _context.Users.FindAsync(order.CustomerId);
                await _mailService.SendOrderChangedEmailAsync(user, order);
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

        private async Task SendRestockEmailAsync(Guid? manufacturerId, Product toOrderProduct)
        {
            if (manufacturerId != null)
            {
                var manufacturer = await _context.Manufacturer.FindAsync(manufacturerId);
                _mailService.SendRestockEmailAsync(manufacturer, toOrderProduct);
            }
            else
            {
                _mailService.SendRestockOrderSelfEmailAsync(toOrderProduct);
            }
            
        }
        
        private static bool IsValidDate(string value, string dateFormats)
        {
            DateTime tempDate;
            bool validDate = DateTime.TryParseExact(value, dateFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out tempDate);
            if (validDate)
                return true;
            else
                return false;
        }

        private async Task sendInvoiceEmail(Order order, string userId)
        {
            var user = await _context.Users
                .Include(u => u.InvoiceAddress)
                .Include(u => u.InvoiceAddress.City)
                .FirstOrDefaultAsync(u => u.Id == userId);

            _mailService.SendInvoiceEmailAsync(user, order);
        }
    }
}
