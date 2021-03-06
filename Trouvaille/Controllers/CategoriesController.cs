using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trouvaille.Models.Communication.Category;
using Trouvaille_WebAPI.Models;

namespace Trouvaille.Controllers
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

                var getCategory = new GetCategoryViewModel(category);
                getCategories.Add(getCategory);
            }
            return Ok(getCategories);
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetCategoryViewModel>> GetCategory(Guid id)
        {
            var category = await _context.Category
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c  => c.CategoryId  == id);
            if (category == null)
            {
                return NotFound();
            }

            ICollection<Guid> products = new List<Guid>();
            /*
            foreach (var VARIABLE in category.Products)
            {
                products.Add(VARIABLE.ProductId);
            }
            */

            var getCategory = new GetCategoryViewModel(category);

            return getCategory;
        }

        // GET: api/Categories/5/10
        [HttpGet("{from}/{to}")]
        public async Task<ActionResult<ICollection<GetCategoryViewModel>>> GetCategoryFromTo(int from, int to)
        {
            if (to <= from)
            {
                return BadRequest("to must be greater then from");
            }
            var category = await _context.Category
                .Skip(from)
                .Take(to - from)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var getCategoryViewModels = category.Select(c => new GetCategoryViewModel(c)).ToList();

            return Ok(getCategoryViewModels);
        }

        // POST: api/Categories/GetMultiple
        [HttpPost]
        [Route("GetMultiple")]
        public async Task<ActionResult<ICollection<GetCategoryViewModel>>> GetMultipleCategories(ICollection<Guid> categoryIds)
        {
            ICollection<GetCategoryViewModel> getCategoryViewModels = new List<GetCategoryViewModel>();
            foreach (var categoryId in categoryIds)
            {
                var category = await _context.Category.FindAsync(categoryId);
                if (category == null)
                {
                    return NotFound($"Category with the ID:{categoryId} not found");
                }
                getCategoryViewModels.Add(new GetCategoryViewModel(category));
            }
            return Ok(getCategoryViewModels);
        }

        /**
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
        **/

        // PUT: api/Categories/{id}/ChangeName
        [HttpPut("{id}/ChangeName")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
        public async Task<IActionResult> PutCategory(Guid id, string name)
        {
            var category = await _context.Category
                .FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            category.Name = name;
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
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
        public async Task<ActionResult<Category>> PostCategory(PostCategoryViewModel model)
        {
            var category = new Category()
            {
                CategoryId = Guid.NewGuid(),
                Name = model.Name,
                ProductCounter = 0
            };

            _context.Category.Add(category);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
               throw;
            }


            var getCategoryViewModel = new GetCategoryViewModel(category);

            return CreatedAtAction("GetCategory", new { id = category.CategoryId }, getCategoryViewModel);
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Category.Remove(category);
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

        private bool CategoryExists(Guid id)
        {
            return _context.Category.Any(e => e.CategoryId == id);
        }

        [HttpGet]
        [Route("Count")]
        public async Task<ActionResult<int>> GetNumberOfCategories()
        {
            var count = await _context.Category.CountAsync();
            return Ok(count);
        }
    }
}
