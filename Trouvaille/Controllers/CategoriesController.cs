using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthoDemoMVC.Data;
using Trouvaille.Models.Communication.Category;
using Trouvaille_WebAPI.Models;

namespace AuthoDemoMVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetCategoryViewModel>>> GetCategory()
        {
            //var categories = await _context.Category.ToListAsync();
            var categories = await _context.Category.Include(c => c.Products).ToListAsync();

            ICollection<GetCategoryViewModel> getCategories = new List<GetCategoryViewModel>();
            foreach (var category in categories)
            {
                ICollection<Guid> productIds = null;
                if (category.Products != null)
                {
                    productIds =
                        category.Products.Select(categoryProduct => categoryProduct.ProductId).ToList();
                }

                var getCategory = new GetCategoryViewModel()
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    ProductIds = productIds
                };
                getCategories.Add(getCategory);
            }
            return Ok(getCategories);
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetCategoryViewModel>> GetCategory(Guid id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            ICollection<Guid> products = new List<Guid>();
            foreach (var VARIABLE in category.Products)
            {
                products.Add(VARIABLE.ProductId);
            }

            var getCategory = new GetCategoryViewModel()
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                ProductIds = products
            };

            return getCategory;
        }

        // PUT: api/Categories/5/AddProducts
        [HttpPut("{id}/AddProducts")]
        public async Task<IActionResult> PutCategory(Guid id, [FromBody]ICollection<Guid> productIds)
        {
            var category = await _context.Category
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            foreach (var productId in productIds)
            {
                var product = await _context.Product.FindAsync(productId);
                if (!category.Products.Contains(product))
                {
                    category.Products.Add(product);
                }
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
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

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(PostCategoryViewModel model)
        {
            var category = new Category()
            {
                CategoryId = Guid.NewGuid(),
                Name = model.Name
            };

            _context.Category.Add(category);
            await _context.SaveChangesAsync();


            var getCategoryViewModel = new GetCategoryViewModel()
            {
                CategoryId = category.CategoryId,
                Name = category.Name
            };

            return CreatedAtAction("GetCategory", new { id = category.CategoryId }, getCategoryViewModel);
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Category.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(Guid id)
        {
            return _context.Category.Any(e => e.CategoryId == id);
        }
    }
}
