using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthoDemoMVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Trouvaille.Services.MailService;

namespace Trouvaille3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMailService _mailService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ApplicationDbContext context, IMailService mailService)
        {
            _logger = logger;
            _context = context;
            _mailService = mailService;
        }

        [HttpGet]
        //public IEnumerable<WeatherForecast> Get()
        public async Task<IActionResult> Get()
        {
            //VERIFY USER ROLE
            //-------------------------------------------
            /**
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            var userRole = await _context.UserRoles.SingleOrDefaultAsync(ur => ur.UserId == userId.Value);

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var value = identity.FindFirst("Role")?.Value;
                if (value != "Customer")
                {
                    return Unauthorized("Not Authorized");
                }
            }
            //-------------------------------------------
            **/

            var rng = new Random();
            var result =  Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            _mailService.SendEmailAsync("yazici98@gmx.de", "Registration", "thank you for your registration");
            return Ok(result);
        }
    }
}
