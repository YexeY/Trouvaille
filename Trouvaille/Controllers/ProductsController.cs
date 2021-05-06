﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Data;
using AuthoDemoMVC.Models.Communication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trouvaille.Models.Communication.Product;
using Trouvaille_WebAPI.Models;

namespace Trouvaille_WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct()
        {
            //return await _context.Product.Include(b => b.ProductCategories).ToListAsync();
            return await _context.Product.ToListAsync();
        }

        // GET: api/Products/10/20
        [HttpGet]
        [Route("{from}/{to}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductFromTo(int from, int to)
        {
            return await _context.Product.Skip(from).Take(to).ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(Guid id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(PostProductViewModel model)
        {
            //Get Manufacturer
            var manufacturer = await _context.Manufacturer.FindAsync(model.ManufacturerId);

            //Create Picture
            var picture = new Picture
            {
                PictureId = Guid.NewGuid(),
                ImageTitle = model.ImageTitle,
                ImageData = model.ImageData
            };

            //GET Categories
            ICollection<Category> categories = new List<Category>();
            if (model.ProductCategoryIds != null)
            {
                foreach (var VARIABLE in model.ProductCategoryIds)
                {
                    var category = await _context.Category.FindAsync(VARIABLE);
                    if (category != null)
                    {
                        categories.Add(category);
                    }
                }
            }
            else
            {
                categories = null;
            }
            //Create Product
            var product = new Product()
            {
                ProductId = Guid.NewGuid(),
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Manufacturer = manufacturer,
                InStock = model.InStock,
                MinStock = model.MinStock,
                Tax = model.Tax,
                picture = picture,
                ProductCategories = categories
            };

            //Add Product and save
            await _context.Product.AddAsync(product);
            await _context.SaveChangesAsync();

            var getProductView = new GetProductViewModel()
            {
                ProductId = product.ProductId,
                Description = product.Description,
                ManufacturerId = product.ManufacturerId,
                InStock = product.InStock,
                picture = product.picture,
                Name = product.Name,
                PictureId = product.PictureId,
                Price = product.Price,
                Tax = product.Tax,
                ProductCategories = product.ProductCategories?.Select(p => p.CategoryId).ToList()
            };

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, getProductView);
        }


        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(Guid id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}
