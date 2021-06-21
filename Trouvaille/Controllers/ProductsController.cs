using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AuthoDemoMVC.Data;
using AuthoDemoMVC.Models.Communication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trouvaille.Models.Communication.Manufacturer;
using Trouvaille.Models.Communication.Product;
using Trouvaille_WebAPI.Models;

namespace Trouvaille.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [Microsoft.AspNetCore.Mvc.HttpGet]
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
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("{from}/{to}")]
        public async Task<ActionResult<ICollection<GetProductViewModel>>> GetProductFromTo(int from, int to, bool onlyActive = true)
        {
            if (onlyActive)
            {
                var products = await _context.Product
                    .Where(p => p.IsDisabled == false)
                    .OrderBy(p => p.ProductId)
                    .Skip(from)
                    .Take((to - from))
                    .Include(p => p.Picture)
                    .ToListAsync();
                var getProductViewModels = products.Select(p => new GetProductViewModel(p)).ToList();

                return Ok(getProductViewModels);
            }
            else
            {
                var products = await _context.Product
                    .OrderBy(p => p.ProductId)
                    .Skip(from)
                    .Take((to - from))
                    .Include(p => p.Picture)
                    .ToListAsync();
                var getProductViewModels = products.Select(p => new GetProductViewModel(p)).ToList();

                return Ok(getProductViewModels);
            }
        }

        /**
        // POST: api/Products/filtered
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("filtered")]
        public async Task<ActionResult<ICollection<GetProductViewModel>>> GetProductFiltered(
            [Microsoft.AspNetCore.Mvc.FromBody] ICollection<Guid> categoryIds)
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

            var getProductViewModels = finalCollection.Select(p => new GetProductViewModel(p)).ToList();
            return Ok(getProductViewModels);
        }
        **/

        // GET: api/Products/5
        [Microsoft.AspNetCore.Mvc.HttpGet("{id}")]
        public async Task<ActionResult<GetProductViewModel>> GetProduct(Guid id)
        {
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
        [Microsoft.AspNetCore.Mvc.HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(Guid id, PutProductViewModel model)
        {
            var product = await _context.Product
                .Include(p => p.Picture)
                .Include(p => p.Manufacturer)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            product.Name = model.Name ?? product.Name;
            product.Description = model.Description ?? product.Description;
            product.MinStock = model.MinStock ?? product.MinStock;
            product.Price = model.Price ?? product.Price;
            product.Tax = model.Tax ?? product.Tax;
            product.InStock = model.InStock ?? product.InStock;
            product.IsDisabled = model.IsDisabled ?? product.IsDisabled;

            //Manufacturer
            if (model.ManufacturerCatalogId != null || model.ManufacturerEmail != null)
            {
                if (product.Manufacturer != null)
                {
                    product.Manufacturer.CatalogId = model.ManufacturerCatalogId ?? product.Manufacturer.CatalogId;
                    product.Manufacturer.Email = model.ManufacturerEmail ?? product.Manufacturer.Email;
                    //_context.Entry(product.Manufacturer).State = EntityState.Modified;
                }
                else
                {
                    product.Manufacturer = new Manufacturer()
                    {
                        CatalogId = model.ManufacturerCatalogId,
                        Email = model.ManufacturerEmail,
                        ManufacturerId = Guid.NewGuid()
                    };
                    _context.Entry(product.Manufacturer).State = EntityState.Added;
                    //product.ManufacturerId = manufacturer.ManufacturerId;
                }


            }

            //Image
            if (model.ImageData != null || model.ImageTitle != null)
            {
                if (product.Picture != null)
                {
                    product.Picture.ImageData = model.ImageData ?? product.Picture.ImageData;
                    product.Picture.ImageTitle = model.ImageTitle ?? product.Picture.ImageTitle;
                    //_context.Entry(product.Picture).State = EntityState.Modified;
                }
                else
                {
                    product.Picture = new Picture()
                    {
                        PictureId = Guid.NewGuid(),
                        ImageData = model.ImageData,
                        ImageTitle = model.ImageTitle
                    };
                    //_context.Entry(product.Picture).State = EntityState.Added;
                    //product.ManufacturerId = manufacturer.ManufacturerId;
                }

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

        // PUT: api/Products/5/image
        [Microsoft.AspNetCore.Mvc.HttpPut]
        [Microsoft.AspNetCore.Mvc.Route("{id}/image")]
        public async Task<IActionResult> PutProductImage(Guid id, PostProductViewModel model)
        {
            var product = await _context.Product
                .Include(p => p.Picture)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return BadRequest();
            }

            //TODO der REST
            if (product.Picture != null)
            {
                product.Picture.ImageTitle = model.ImageTitle;
                product.Picture.ImageData = model.ImageData;

                _context.Entry(product.Picture).State = EntityState.Modified;
            }
            else
            {
                product.Picture = new Picture()
                {
                    ImageData = model.ImageData,
                    ImageTitle = model.ImageTitle,
                    PictureId = Guid.NewGuid()
                };
                _context.Entry(product).State = EntityState.Modified;
            }

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
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("GetMultiple")]
        public async Task<ActionResult<ICollection<GetProductViewModel>>> GetMultipleProducts([Microsoft.AspNetCore.Mvc.FromBody] ICollection<Guid> productIds)
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
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("Count")]
        public async Task<ActionResult<int>> GetNumberOfProducts(bool onlyActive = true)
        {
            if (onlyActive)
            {
                var count = await _context.Product.Where(p => p.IsDisabled == false).CountAsync();
                return Ok(count);
            }
            else
            {
                var count = await _context.Product.CountAsync();
                return Ok(count);
            }
        }

        // POST: api/Products/5/addCategory
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("{id}/addCategory")]
        public async Task<IActionResult> AddCategoryToProduct(Guid id,[Microsoft.AspNetCore.Mvc.FromBody]ICollection<Guid> categoryIds)
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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            { 
                throw;
            }
            return Ok();
        }

        // POST: api/Products/5/deleteCategory
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("{id}/deleteCategory")]
        public async Task<IActionResult> DeleteCategoryToProduct(Guid id, [Microsoft.AspNetCore.Mvc.FromBody] ICollection<Guid> categoryIds)
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
                if (product.ProductCategories.Any(c => c.CategoryId == categoryId) == true)
                {
                    var category = await _context.Category.FindAsync(categoryId);
                    if (category == null)
                    {
                        return NotFound($"Category with id ={categoryId} not found");
                    }
                    product.ProductCategories.Remove(category);
                    category.ProductCounter -= 1;
                    _context.Entry(category).State = EntityState.Modified;
                }
            }

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: api/Products
        [Microsoft.AspNetCore.Mvc.HttpPost]
        public async Task<ActionResult<Product>> PostProduct(PostProductViewModel model)
        {
            //Get Manufacturer
            var manufacturer = new Manufacturer()
            {
                ManufacturerId = Guid.NewGuid(),
                Email = model.ManufacturerEmail,
                CatalogId = model.ManufacturerCatalogId
            };

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
            try
            {
                await _context.Product.AddAsync(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            var getProductView = new GetProductViewModel(product);

            return CreatedAtAction("GetProduct", new { id = product.ProductId }, getProductView);
        }

        /**
        // DELETE: api/Products/5
        [Microsoft.AspNetCore.Mvc.HttpDelete("{id}")]
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
        **/

        // POST: api/Products/SearchQuery/5/10
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("SearchQuery/{from}/{to}")]
        public async Task<ActionResult<ICollection<GetProductViewModel>>> SearchQueryProduct(int from, int to, string searchWord = "",
              bool asc = true, ICollection<Guid>? categoryIds = null, string orderBy = "Price", bool onlyActive = true)
        {
            StringBuilder query = new StringBuilder();
            //Check Order By

            if (categoryIds != null && categoryIds.Count > 0)
            {
                query.AppendLine("  select * from Product P where ProductId IN (");
                //---------------------------------
                query.AppendLine(
                    $"  select ProductsProductId from CategoryProduct  where ProductCategoriesCategoryId = '{categoryIds.ElementAt(0).ToString()}' ");
                for (int i = 1; i < categoryIds.Count; i++)
                {
                    var categoryId = categoryIds.ElementAt(i).ToString();
                    query.AppendLine("  Intersect");
                    query.AppendLine(
                        $"  select ProductsProductId from CategoryProduct where ProductCategoriesCategoryId = '{categoryIds.ElementAt(i).ToString()}'");
                }

                query.AppendLine("  )");
                query.AppendLine("  and (");
                query.AppendLine($"     P.Description	LIKE	'%{searchWord}%'");
                query.AppendLine($"  OR	P.Name			LIKE	'%{searchWord}%'");
                query.AppendLine("  )");
                if (onlyActive)
                {
                    query.AppendLine("  and IsDisabled = 0");
                }
            }
            else
            {
                query.AppendLine("  select * from Product P where");
                query.AppendLine($"     P.Description	LIKE	'%{searchWord}%'");
                query.AppendLine($"  OR	P.Name			LIKE	'%{searchWord}%'");
                if (onlyActive)
                {
                    query.AppendLine("  and IsDisabled = 0");
                }
            }

            var products = new List<Product>();
            if (asc)
            {
                products = await _context.Product.FromSqlRaw(query.ToString())
                    .Include(p => p.Picture)
                    .OrderBy(p => p.Price)
                    .Skip(from)
                    .Take(to - from)
                    .ToListAsync();
            }
            else
            {
                products = await _context.Product.FromSqlRaw(query.ToString())
                    .Include(p => p.Picture)
                    .OrderByDescending(p => p.Price)
                    .Skip(from)
                    .Take(to - from)
                    .ToListAsync();
            }

            var getProductsViewModels = new List<GetProductViewModel>();
            foreach (var product in products)
            {
                if (product == null)
                {
                    return NotFound("product not found");
                }
                getProductsViewModels.Add(new GetProductViewModel(product));
            }
            return Ok(getProductsViewModels);
        }


        // Get: api/Manufacturer/{id}
        [Microsoft.AspNetCore.Mvc.HttpGet("/api/Manufacturer/{id}")]
        public async Task<ActionResult<GetManufacturerViewModel>> GetManufacturer(Guid id)
        {
            var manufacturer = await _context.Manufacturer.FindAsync(id);

            return Ok(new GetManufacturerViewModel(manufacturer));
        }

        private bool ProductExists(Guid id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}
