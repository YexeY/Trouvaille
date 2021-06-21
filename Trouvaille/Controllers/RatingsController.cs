using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthoDemoMVC.Data;
using Microsoft.AspNetCore.Authorization;
using Trouvaille.Models.Communication.Rating;
using Trouvaille_WebAPI.Models;

namespace Trouvaille.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RatingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Ratings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetRatingViewModel>>> GetRating()
        {
            var ratings = await _context.Rating.ToListAsync();
            var getRatingViewModels = ratings.Select(r => new GetRatingViewModel(r)).ToList();
            return getRatingViewModels;
        }

        [HttpGet]
        [Route("{from}/{to}")]
        public async Task<ActionResult<IEnumerable<GetRatingViewModel>>> GetRatingsFromTo(int from, int to)
        {
            var ratings = await _context.Rating
                .OrderBy(r => r.ProductId)
                .Skip(from)
                .Take(to - from)
                .ToListAsync();

            var getRatingViewModels = ratings.Select(r => new GetRatingViewModel(r)).ToList();
            return getRatingViewModels;
        }

        // GET: api/Ratings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Rating>> GetRating(Guid id)
        {
            var rating = await _context.Rating.FindAsync(id);

            if (rating == null)
            {
                return NotFound();
            }

            return rating;
        }

        // GET: api/Ratings/Product/{ProductID}/10/15
        [HttpGet]
        [Route("Product/{id}/{from}/{to}")]
        public async Task<ActionResult<IEnumerable<GetRatingViewModel>>> GetRatingOfProduct(int from, int to, Guid productId)
        {
            var query = new StringBuilder();
            var product = await _context.Product
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound();
            }

            query.AppendLine($" select * from Rating R");
            query.AppendLine($" where R.ProductId = '{productId.ToString()}'");


            var ratings = await _context.Rating.FromSqlRaw(query.ToString())
                .OrderBy(r => r.StarCount)
                .Skip(from)
                .Take(to - from)
                .ToListAsync();
            var getRatingViewModels = ratings.Select(r => new GetRatingViewModel(r)).ToList();

            return Ok(getRatingViewModels);
        }

        // GET: api/Ratings/Customer/{CustomerID}/10/15
        [HttpGet]
        [Route("Customer/{id}/{from}/{to}")]
        public async Task<ActionResult<IEnumerable<GetRatingViewModel>>> GetRatingOfCustomer(int from, int to, Guid customerId)
        {
            var query = new StringBuilder();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == customerId.ToString());
            if (user == null)
            {
                return NotFound();
            }
            query.AppendLine($" select * from Rating R");
            query.AppendLine($" where R.CustomerId = '{customerId.ToString()}'");

            var ratings = await _context.Rating.FromSqlRaw(query.ToString())
                .OrderBy(r => r.StarCount)
                .Skip(from)
                .Take(to - from)
                .ToListAsync();
            var getRatingViewModels = ratings.Select(r => new GetRatingViewModel(r)).ToList();

            return Ok(getRatingViewModels);
        }

        // PUT: api/Ratings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRating(Guid id, PutRatingViewModel putRatingViewModel)
        {
            var rating = await _context.Rating.FindAsync(id);

            rating.StarCount = putRatingViewModel.StarCount ?? rating.StarCount;
            rating.Description = putRatingViewModel.Description ?? rating.Description;
            rating.Title = putRatingViewModel.Title ?? rating.Title;

            _context.Entry(rating).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RatingExists(id))
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

        // POST: api/Ratings
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<GetRatingViewModel>> PostRating(PostRatingViewModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.Products).FirstOrDefaultAsync(u => u.Id == userId.Value);
            var userProductIds = user.Products?.Select(u => u.ProductId).ToList();

            if (userProductIds?.Contains(model.ProductId) != true)
            {
                return Forbid("User did not Order this Product before");
            }

            var rating = new Rating()
            {
                RatingId = Guid.NewGuid(),
                CustomerId = userId?.Value,
                Description = model.Description,
                ProductId = model.ProductId,
                StarCount = model.StarCount,
                Title = model.Title,
                Customer = null,
                Product = null
            };

            var product = await _context.Product.FindAsync(model.ProductId);
            product.AverageRating =
                    (decimal) (((product.RatingCounter * product.AverageRating) + rating.StarCount) /
                               (product.RatingCounter + 1));
            product.RatingCounter += 1;
            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.Rating.AddAsync(rating);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            var getRatingVieModel = new GetRatingViewModel(rating);

            return CreatedAtAction("GetRating", new { id = rating.RatingId }, getRatingVieModel);
        }

        // DELETE: api/Ratings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRating(Guid id)
        {
            var rating = await _context.Rating.FindAsync(id);
            if (rating == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(rating.ProductId);
            product.RatingCounter -= 1;
            product.AverageRating = (product.AverageRating * (product.RatingCounter + 1) - rating.StarCount) /
                                    product.AverageRating;
            _context.Entry(product).State = EntityState.Modified;

            
            try
            {
                _context.Rating.Remove(rating);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }


        [HttpGet]
        [Route("CanRate/{id}")]
        public async Task<IActionResult> CanRate(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.Products).FirstOrDefaultAsync(u => u.Id == userId.Value);
            var userProduct = user.Products?.Select(u => u.ProductId).ToList();

            return Ok(userProduct?.Contains(id) == true);
        }

        private bool RatingExists(Guid id)
        {
            return _context.Rating.Any(e => e.RatingId == id);
        }
    }
}
