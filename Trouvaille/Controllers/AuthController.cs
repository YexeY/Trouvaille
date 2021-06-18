using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AuthoDemoMVC.Data;
using AuthoDemoMVC.Data.CustomerService;
using AuthoDemoMVC.Data.EmployeeService;
using AuthoDemoMVC.Data.UserService;
using AuthoDemoMVC.Models.Communication;
using AuthoDemoMVC.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Trouvaille.Models.Communication.Customer;
using Trouvaille.Models.Communication.Product;

namespace Trouvaille3.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly ApplicationDbContext _context;

        public AuthController(IUserService userService, ICustomerService customerService, IEmployeeService employeeService, ApplicationDbContext context)
        {
            _userService = userService;
            _customerService = customerService;
            _employeeService = employeeService;
            _context = context;
        }

        // POST: api/auth/customer/register
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("Customer/Register")]
        public async Task<IActionResult> RegisterCustomerAsync([Microsoft.AspNetCore.Mvc.FromBody] RegisterCustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.RegisterCustomerAsync(model);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }

            return BadRequest("Some properties are not valid"); // Status Code: 400
        }

        // POST: api/auth/customer/login
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("Customer/Login")]
        public async Task<IActionResult> LoginCustomerAsync([Microsoft.AspNetCore.Mvc.FromBody] LoginCustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.LoginCustomerAsync(model);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }

            return BadRequest("Some properties are not Valid"); //TODO code 401 not authorized
        }

        // POST: api/auth/Employee/register
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("Employee/Register")]
        public async Task<IActionResult> RegisterEmployeeAsync([Microsoft.AspNetCore.Mvc.FromBody] RegisterEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _employeeService.RegisterEmployeeAsync(model);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }

            return BadRequest("Some properties are not valid"); // Status Code: 400
        }

        // POST: api/auth/Employee/login
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("Employee/Login")]
        public async Task<IActionResult> LoginEmployeeAsync([Microsoft.AspNetCore.Mvc.FromBody] LoginEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _employeeService.LoginEmployeeAsync(model);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }

            return BadRequest("Some properties are not Valid"); //TODO code 401 not authorized
        }


        // GET: api/auth/Customer/info
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("Customer/info")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetCustomerInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            var result = await _customerService.GetCustomerInfo(userId.Value);

            return Ok(result);
        }

        // GET: api/auth/Customer/5/10
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("Customer/{from}/{to}")]
        public async Task<ActionResult<ICollection<GetCustomerViewModel>>> GetCustomers(int from, int to)
        {
            StringBuilder query = new StringBuilder();


            query.AppendLine("  select * from AspNetUsers where Id IN (");
            query.AppendLine("  select R.UserId from AspNetUserRoles R");
            query.AppendLine("  where R.RoleId = 1");
            query.AppendLine("  ) order by Id asc");
            query.AppendLine($"OFFSET {from} ROWS");
            query.AppendLine($"FETCH NEXT {to - from} ROWS ONLY");

            var customers = await _context.Users.FromSqlRaw(query.ToString())
                //.Skip(from)
                //.Take((to - from))
                                .Include(c => c.DeliveryAddress)
                                .Include(c => c.InvoiceAddress)
                                .Include(c => c.Orders)
                                .Include(c => c.InvoiceAddress.City)
                                .Include(c => c.DeliveryAddress.City)
                .ToListAsync();

            var getCustomerViewModels = new List<GetCustomerViewModel>();
            foreach (var customer in customers)
            {
                if (customer == null)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Employee doesn't exist", System.Text.Encoding.UTF8, "text/plain"),
                        StatusCode = HttpStatusCode.NotFound
                    };
                    throw new HttpResponseException(response);
                }
                getCustomerViewModels.Add(new GetCustomerViewModel(customer));
            }
            return Ok(getCustomerViewModels);
        }

        // GET: api/auth/Customer/5/10
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("Customer/SearchQuery/{from}/{to}")]
        public async Task<ActionResult<ICollection<GetCustomerViewModel>>> GetCustomersSearchQuery(int from, int to, Guid? customerId, string? customerEmail)
        {
            StringBuilder query = new StringBuilder();


            query.AppendLine("  select * from AspNetUsers U where Id IN (");
            query.AppendLine("  select R.UserId from AspNetUserRoles R");
            query.AppendLine("  where R.RoleId = 1");
            query.AppendLine("  ) ");
            if (customerId != null)
            {
                query.AppendLine($"  and U.Id = '{customerId.ToString()}' ");
            }
            if (customerEmail != null)
            {
                query.AppendLine($"  and U.Email = '{customerEmail}' ");
            }
            query.AppendLine("  order by Id asc");
            query.AppendLine($"OFFSET {from} ROWS");
            query.AppendLine($"FETCH NEXT {to - from} ROWS ONLY");

            var customers = await _context.Users.FromSqlRaw(query.ToString())
                //.Skip(from)
                //.Take((to - from))
                .Include(c => c.DeliveryAddress)
                .Include(c => c.InvoiceAddress)
                .Include(c => c.Orders)
                .Include(c => c.InvoiceAddress.City)
                .Include(c => c.DeliveryAddress.City)
                .ToListAsync();

            var getCustomerViewModels = new List<GetCustomerViewModel>();
            foreach (var customer in customers)
            {
                if (customer == null)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Employee doesn't exist", System.Text.Encoding.UTF8, "text/plain"),
                        StatusCode = HttpStatusCode.NotFound
                    };
                    throw new HttpResponseException(response);
                }
                getCustomerViewModels.Add(new GetCustomerViewModel(customer));
            }
            return Ok(getCustomerViewModels);
        }

        // GET: api/auth/Customer/Count
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("Count")]
        public async Task<ActionResult<int>> GetNumberOfCustomers()
        {
            string query =   " select Count(*) from AspNetUsers U where Id IN ("
                           + " select R.UserId from AspNetUserRoles R"
                           + " where R.RoleId = 1"
                           + " )";

            var count = await _context.Database.ExecuteSqlRawAsync(query);
            return Ok(count);
        }


        


        /**
        // POST: api/auth/register
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.RegisterUserAsync(model);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }

            return BadRequest("Some properties are not valid"); // Status Code: 400
        }

        // POST: api/auth/login
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.LoginUserAsync(model);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }

            return BadRequest("Some properties are not Valid"); //TODO code 401 not authorized
        }
        **/
    }
}
