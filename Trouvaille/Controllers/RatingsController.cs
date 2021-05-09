using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        // PUT: api/Ratings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRating(Guid id, Rating rating)
        {
            if (id != rating.RatingId)
            {
                return BadRequest();
            }

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
            var userProduct = user?.Products?.Select(u => u.ProductId).ToList();

            if (userProduct?.Contains(model.ProductId) != true)
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

            await _context.Rating.AddAsync(rating);
            await _context.SaveChangesAsync();

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

            _context.Rating.Remove(rating);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RatingExists(Guid id)
        {
            return _context.Rating.Any(e => e.RatingId == id);
        }
    }
}
