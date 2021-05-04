﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthoDemoMVC.Data;
using AuthoDemoMVC.Models;
using AuthoDemoMVC.Models.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Trouvaille_WebAPI.Models;

namespace AuthoDemoMVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrder()
        {
            return await _context.Order.ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(Guid id)
        {
            var order = await _context.Order.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Order>> PostOrder(PostOrderViewModel model)
        {


            //Extract Adresses
            //------------------------------------------
            var invoiceCity = new City
            {
                CityId = Guid.NewGuid(),
                Name = model.InvoiceAddress.CityName,
                PostalCode = model.InvoiceAddress.PostalCode
            };

            var invoiceAddress = new Address
            {
                AddressId = Guid.NewGuid(),
                Country = model.InvoiceAddress.Country,
                State = model.InvoiceAddress.State,
                Street = model.InvoiceAddress.Street,
                StreetNumber = model.InvoiceAddress.StreetNumber,
                CityId = invoiceCity.CityId,
                City = invoiceCity
            };

            City deliveryCity;
            Address deliveryAddress;

            if (model.DeliveryAddress == null)
            {
                deliveryAddress = invoiceAddress;
            }
            else
            {
                deliveryCity = new City
                {
                    CityId = Guid.NewGuid(),
                    Name = model.DeliveryAddress.CityName,
                    PostalCode = model.DeliveryAddress.PostalCode
                };

                deliveryAddress = new Address
                {
                    AddressId = Guid.NewGuid(),
                    Country = model.DeliveryAddress.Country,
                    State = model.DeliveryAddress.State,
                    Street = model.DeliveryAddress.Street,
                    StreetNumber = model.DeliveryAddress.StreetNumber,
                    CityId = deliveryCity.CityId,
                    City = deliveryCity
                };
            }
            //------------------------------------------

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                Invoice_Id = Guid.NewGuid(),
                Date = model.Date,
                PaymentMethod = model.PaymentMethod,
                ShipmentMethod = model.ShipmentMethod,
                OrderState = model.OrderState,
                InvoiceAddress = invoiceAddress,
                DeliveryAddress = deliveryAddress,
               // Products = 
            };

            //Extract Products and add to Order
            //------------------------------------------
            ICollection<OrderProduct> orderProducts = new List<OrderProduct>();
            //ICollection<Product> products = new List<Product>();
            foreach (var VARIABLE in model.Products)
            {
                var product = _context.Product.Find(VARIABLE.ProductId);
                if (product == null)
                {
                    continue;
                }
                var cardinality = VARIABLE.Cardinality;
                if (product.InStock - cardinality < product.MinStock)
                {
                    //TODO sth
                    Console.WriteLine("Dont have that many in stock");
                }
                else
                {
                    product.InStock -= cardinality;
                    _context.Product.Update(product);
                }

                var orderProduct = new OrderProduct
                {
                    Product = product,
                    Cardinality = cardinality,
                    OderProductId = Guid.NewGuid(),
                    ProductId = product.ProductId,
                    Order = order,
                    OrderId = order.OrderId
                };

                //products.Add(product);
                orderProducts.Add(orderProduct);
            }
            order.Products = orderProducts;
            //------------------------------------------
            _context.Order.Add(order);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
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
    }
}
