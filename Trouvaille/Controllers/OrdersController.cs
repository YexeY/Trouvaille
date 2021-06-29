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
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
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
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
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
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
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
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsActiveCustomer")]
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<ActionResult<GetOrderViewModel>> PostOrder(PostOrderViewModel model)
        {
            //VERIFY USER ROLE
            //-------------------------------------------
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.Products).FirstOrDefaultAsync(u => u.Id == userId.Value);
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
            if(model.Products == null || model.Products.Count == 0)
            {
                return BadRequest("List of Product cant be null or empty");
            }

            var counter = 0;
            ICollection<SendRestockEmailParam> sendRestockEmailParams = new List<SendRestockEmailParam>();
            ICollection<OrderProduct> orderProducts = new List<OrderProduct>();
            ICollection<Product> products = new List<Product>();
            foreach (var VARIABLE in model.Products)
            {
                var product = await _context.Product.FindAsync(VARIABLE.ProductId);
                counter += VARIABLE.Cardinality;
                if (product == null)
                {
                    continue;
                }

                var cardinality = VARIABLE.Cardinality;

                if (product.InStock - cardinality < 0)
                {
                    return BadRequest("Don't have that many in stock");
                }

                if (product.IsDisabled)
                {
                    return BadRequest($"Product with ID: {product.ProductId} is Disabled");
                }

                if ((product.InStock - cardinality) < product.MinStock)
                {
                    sendRestockEmailParams.Add(new SendRestockEmailParam()
                    {
                        ManufacturerId = product.ManufacturerId,
                        ProductId = product.ProductId
                    });
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
            if(counter <= 0)
            {
                return BadRequest("You have to Order sth");
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

            try
            {
                await _mailService.SendOrderConfirmationEmailAsync(user, order);
                await _mailService.SendInvoiceEmailAsync(user, order);
                await SendRestockEmailManyAsync(sendRestockEmailParams);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            var getOrderViewModel = new GetOrderViewModel(order);
            return CreatedAtAction("GetOrder", new { id = order.OrderId }, getOrderViewModel);
        }

        // DELETE: api/Orders/5
        [Microsoft.AspNetCore.Mvc.HttpDelete("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
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
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
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
                /**
                if (!IsValidDate(fromDateTime, dateFormats))
                {
                    return BadRequest("Bad DateTime format");
                }
                **/
                query.AppendLine(and ? " and " : "");
                query.AppendLine($"  Date >= '{fromDateTime}'");
                and = true;
            }
            if (toDateTime != null)
            {
                /**
                if (!IsValidDate(toDateTime, dateFormats))
                {
                    return BadRequest("Bad DateTime format");
                }
                **/
                query.AppendLine(and ? " and " : "");
                query.AppendLine($"  Date <= '{toDateTime}'");
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
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
        public async Task<ActionResult<int>> GetNumberOfOrders()
        {
            var count = await _context.Order.CountAsync();
            return Ok(count);
        }

        //Post: api/Orders/History/0/5
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("History/{from}/{to}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsUser")]
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

            var getOrderViewModels = orders.Select(o => new GetOrderViewModel(o)).ToList();

            return Ok(getOrderViewModels);
        }


        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("History/Count")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsUser")]
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
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
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



        private async Task SendRestockEmailAsync(SendRestockEmailParam param)
        {
            var product = _context.Product.Find(param.ProductId);
            if (param.ManufacturerId != null)
            {
                bool success = false;
                var manufacturer = _context.Manufacturer.Find(param.ManufacturerId);
                if(manufacturer != null)
                {
                    success = await _mailService.SendRestockEmailAsync(manufacturer, product);
                }
                await _mailService.SendRestockOrderSelfEmailAsync(product, success);
            }
            else
            {
                await _mailService.SendRestockOrderSelfEmailAsync(product, false);
            }
            
        }

        private async Task SendRestockEmailManyAsync(ICollection<SendRestockEmailParam> param)
        {
            var products = param.Select(p => _context.Product
            .Include(c => c.Manufacturer)
            .FirstOrDefault(c => c.ProductId == p.ProductId))
            .ToList();
            await _mailService.SendRestockOrderSelfManyEmailAsync(products);
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
                .FirstOrDefaultAsync(u => u.Id == userId);

            await _mailService.SendInvoiceEmailAsync(user, order);
        }

        private class SendRestockEmailParam
        {
            public Guid? ManufacturerId { get; set; }
            public Guid ProductId { get; set; }
        }
    }
}
