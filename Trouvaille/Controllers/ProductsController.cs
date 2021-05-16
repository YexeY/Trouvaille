using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public async Task<ActionResult<ICollection<GetProductViewModel>>> GetProduct()
        {
            var products = await _context.Product
                .Include(b => b.ProductCategories)
                .Include(p => p.Picture)
                .Include(p => p.Ratings)
                .ToListAsync();
            ICollection<GetProductViewModel> getProductViewModels = new List<GetProductViewModel>();
            foreach (var product in products)
            {
                var productView = new GetProductViewModel(product);
                getProductViewModels.Add(productView);
            }

            return Ok(getProductViewModels);
        }

        // GET: api/Products/10/20
        [HttpGet]
        [Route("{from}/{to}")]
        public async Task<ActionResult<IEnumerable<GetProductViewModel>>> GetProductFromTo(int from, int to)
        {
            var products = await _context.Product
                .Skip(from)
                .Take((to - from))
                //.Include(b => b.ProductCategories)
                .Include(p => p.Picture)
                //.Include(p => p.Ratings)
                .ToListAsync();
            ICollection<GetProductViewModel> getProductViewModels = new List<GetProductViewModel>();
            foreach (var product in products)
            {
                var productView = new GetProductViewModel(product);
                getProductViewModels.Add(productView);
            }

            return Ok(getProductViewModels);
        }

        // POST: api/Products/filtered
        [HttpPost]
        [Route("filtered")]
        public async Task<ActionResult<ICollection<GetProductViewModel>>> GetProductFiltered(
            [FromBody] ICollection<Guid> categoryIds)
        {
            IEnumerable<Product> finalCollection = null;
            foreach (var categoryId in categoryIds)
            {
                var products =
                    await _context.Product
                        .Include(b => b.ProductCategories)
                        .Include(p => p.Picture)
                        .Include(p => p.Ratings)
                        .Where(p => p.ProductCategories.Contains(
                            _context.Category.FirstOrDefault(c => c.CategoryId == categoryId))).ToListAsync();

                finalCollection ??= products;
                finalCollection = finalCollection.Intersect(products);

            }

            if (finalCollection == null) return Ok((ICollection<Product>) null);

            ICollection<GetProductViewModel> getProductViews = new List<GetProductViewModel>();
            foreach (var product in finalCollection)
            {
                var productViewModel = new GetProductViewModel(product);
                getProductViews.Add(productViewModel);
            }

            return Ok(getProductViews);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetProductViewModel>> GetProduct(Guid id)
        {
            //var product = await _context.Product.Include(b => b.ProductCategories).FindAsync(id);
            var product = await _context.Product
                .Include(b => b.ProductCategories)
                .Include(p => p.Picture)
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var getProductViewModel = new GetProductViewModel(product);
            return getProductViewModel;
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(Guid id, PostProductViewModel model)
        {
            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return BadRequest();
            }

            //TODO der REST
            product.Name = model.Name;
            product.Description = model.Description;
            product.MinStock = model.MinStock;
            product.Price = model.Price;
            product.Tax = model.Tax;
            product.InStock = model.InStock;

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


        // POST: api/Products/GetMultiple
        [HttpPost]
        [Route("GetMultiple")]
        public async Task<ActionResult<GetProductViewModel>> GetMultipleProducts([FromBody] ICollection<Guid> productIds)
        {
            ICollection<GetProductViewModel> getProductViewModels = new List<GetProductViewModel>();
            foreach (var productId in productIds)
            {
                var product = await _context.Product
                    .Include(b => b.ProductCategories)
                    .Include(p => p.Picture)
                    .Include(p => p.Ratings)
                    .FirstOrDefaultAsync(p => p.ProductId == productId);
                if (product == null)
                {
                    return NotFound($"Product with the ID:{productId} not found");
                }
                getProductViewModels.Add(new GetProductViewModel(product));
            }

            return Ok(getProductViewModels);
        }

        // GET: api/Products/Count
        [HttpGet]
        [Route("Count")]
        public async Task<ActionResult<int>> GetNumberOfProducts()
        {
            var count = await _context.Product.CountAsync();
            return Ok(count);
        }

        // POST: api/Product/5/addCategory
        [HttpPost]
        [Route("{id}/addCategory")]
        public async Task<IActionResult> AddCategoryToProduct(Guid id,[FromBody]ICollection<Guid> categoryIds)
        {
            var product = await _context.Product
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound($"Product with id:{id} not found");
            }

            foreach (var categoryId in categoryIds)
            {
                if (product.ProductCategories.Any(c => c.CategoryId == categoryId) == false)
                {
                    var category = await _context.Category.FindAsync(categoryId);
                    if (category == null)
                    {
                        return NotFound($"Category with id ={categoryId} not found");
                    }
                    product.ProductCategories.Add(category);
                    category.ProductCounter += 1;
                    _context.Entry(category).State = EntityState.Modified;
                }
            }

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
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
                Picture = picture,
                ProductCategories = categories,
                RatingCounter = 0
            };

            //Add Product and save
            await _context.Product.AddAsync(product);
            await _context.SaveChangesAsync();

            var getProductView = new GetProductViewModel(product);

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
