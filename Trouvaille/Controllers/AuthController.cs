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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trouvaille.Models.Communication.Base;
using Trouvaille.Models.Communication.Customer;
using Trouvaille.Models.Communication.Employee;

namespace Trouvaille.Controllers
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
            if (ModelState.IsValid && validateCustomerWithEmail(model.Email))
            {
                var result = await _customerService.LoginCustomerAsync(model);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return Unauthorized(result);
            }

            return Unauthorized("Some properties are not Valid");
        }

        // POST: api/auth/Employee/register
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("Employee/Register")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsAdmin")]
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

                return Unauthorized(result);
            }

            return Unauthorized("Some properties are not Valid"); //TODO code 401 not authorized
        }


        [Microsoft.AspNetCore.Mvc.HttpDelete]
        [Microsoft.AspNetCore.Mvc.Route("Employee/Delete/{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> DeleteEmployeeAsync(Guid id)
        {
            var userRole = _context.UserRoles.SingleOrDefault(ur => ur.UserId == id.ToString());
            var role = _context.Roles.FirstOrDefault(x => x.Id == userRole.RoleId);
            
            if (role.Name == "Employee")
            {
                var customer = await _context.Users.FindAsync(id.ToString());
                _context.Users.Remove(customer);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
            return Ok();
        }
    

        // GET: api/auth/Customer/info
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("Customer/info")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsActiveCustomer")]
        public async Task<IActionResult> GetCustomerInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!validateCustomerWithId(userId.Value))
            {
                return Unauthorized();
            }
            var result = await _customerService.GetCustomerInfo(userId.Value);

            return Ok(result);
        }

        // GET: api/auth/Customer/5/10
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("Customer/{from}/{to}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
        public async Task<ActionResult<ICollection<GetCustomerViewModel>>> GetCustomers(int from, int to, bool onlyActive = true)
        {
            if (to <= from)
            {
                return BadRequest("to must be greater then from");
            }
            StringBuilder query = new StringBuilder();
            query.AppendLine("  select * from AspNetUsers where Id IN (");
            query.AppendLine("  select R.UserId from AspNetUserRoles R");
            query.AppendLine("  where R.RoleId = 1");
            query.AppendLine("  )");
            if (onlyActive)
            {
                query.AppendLine("  and IsDisabled = 0");
            }

            /**
            query.AppendLine("  order by Id asc");
            query.AppendLine($" OFFSET {from} ROWS");
            query.AppendLine($" FETCH NEXT {to - from} ROWS ONLY");
            **/
            
            var customers = await _context.Users.FromSqlRaw(query.ToString())
                .Include(c => c.DeliveryAddress)
                .Include(c => c.InvoiceAddress)
                .Include(c => c.Orders)
                .Include(c => c.InvoiceAddress.City)
                .Include(c => c.DeliveryAddress.City)
                .OrderBy( c => c.Id)
                .Skip(from)
                .Take(to - from)
                .ToListAsync();

            var getCustomerViewModels = new List<GetCustomerViewModel>();
            foreach (var customer in customers)
            {
                if (customer == null)
                {
                    return NotFound("Customer doesn't exist");
                }
                getCustomerViewModels.Add(new GetCustomerViewModel(customer));
            }
            return Ok(getCustomerViewModels);
        }

        // GET: api/auth/Customer/5/10
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("Customer/SearchQuery/{from}/{to}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
        public async Task<ActionResult<ICollection<GetCustomerViewModel>>> GetCustomersSearchQuery(int from, int to, Guid? customerId, string? customerEmail, bool onlyActive = true)
        {
            if (to <= from)
            {
                return BadRequest("to must be greater then from");
            }
            StringBuilder query = new StringBuilder();
            query.AppendLine("  select * from AspNetUsers U where Id IN (");
            query.AppendLine("  select R.UserId from AspNetUserRoles R");
            query.AppendLine("  where R.RoleId = 1");
            query.AppendLine("  ) ");
            if (customerId != null)
            {
                query.AppendLine($"  and U.Id LIKE '%{customerId.ToString()}%' ");
            }
            if (customerEmail != null)
            {
                query.AppendLine($"  and U.Email LIKE '%{customerEmail}%' ");
            }
            if (onlyActive)
            {
                query.AppendLine("  and IsDisabled = 0");
            }

            var customers = await _context.Users.FromSqlRaw(query.ToString())
                .Include(c => c.DeliveryAddress)
                .Include(c => c.InvoiceAddress)
                .Include(c => c.Orders)
                .Include(c => c.InvoiceAddress.City)
                .Include(c => c.DeliveryAddress.City)
                .OrderBy(c => c.Id)
                .Skip(from)
                .Take(to - from)
                .ToListAsync();

            var getCustomerViewModels = new List<GetCustomerViewModel>();
            foreach (var customer in customers)
            {
                if (customer == null)
                {
                    return NotFound("Customer doesn't exist");
                }
                getCustomerViewModels.Add(new GetCustomerViewModel(customer));
            }
            return Ok(getCustomerViewModels);
        }

        // GET: api/auth/Customer/Count
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("Customer/Count")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
        public async Task<ActionResult<int>> GetNumberOfCustomers(bool onlyActive = true)
        {
            var customerIds = await _context.UserRoles.Where(u => u.RoleId == "1").Select(u => u.UserId).ToListAsync();
            int count;
            if (onlyActive)
            {
                count = await _context.Users.Where(u => u.IsDisabled == false && customerIds.Contains(u.Id)).CountAsync();
            }
            else
            {
                count = await _context.Users.Where(u => customerIds.Contains(u.Id)).CountAsync();
            }

            return Ok(count);
        }

         // GET: api/auth/Employee/Count
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("Employee/Count")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsAdmin")]
        public async Task<ActionResult<int>> GetNumberOfEmployees(bool onlyActive = true)
        {
            var employeeIds = await _context.UserRoles.Where(u => u.RoleId == "2").Select(u => u.UserId).ToListAsync();
            int count;
            if (onlyActive)
            {
                count = await _context.Users.Where(u => u.IsDisabled == false && employeeIds.Contains(u.Id)).CountAsync();
            }
            else
            {
                count = await _context.Users.Where(u => employeeIds.Contains(u.Id)).CountAsync();
            }

            return Ok(count);
        }

        // PUT: api/auth/Employee
        [Microsoft.AspNetCore.Mvc.HttpPut]
        [Microsoft.AspNetCore.Mvc.Route("Employee")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsAdmin")]
        public async Task<ActionResult<GetEmployeeViewModel>> PutEmployee([Microsoft.AspNetCore.Mvc.FromBody] PutEmployeeViewModel putEmployeeViewModel, Guid employeeId)
        {
            var id = employeeId.ToString();
            

            var employee = await _context.Users.FindAsync(id);
            if (employee == null)
            {
                return NotFound("employee not found");
            }

            var role = await _context.UserRoles.FirstOrDefaultAsync(r => r.UserId == employee.Id);
            if (role == null || role.RoleId != "2")
            {
                return NotFound("not an employee");
            }

            employee.Email = putEmployeeViewModel.Email ?? employee.Email;
            employee.FirstName = putEmployeeViewModel.FirstName ?? employee.FirstName;
            employee.LastName = putEmployeeViewModel.LastName ?? employee.LastName;
            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                //TODO: proper error handling
                Console.WriteLine(e);
                throw;
            }

            var getEmployeeViewModel = new GetEmployeeViewModel(employee);
            return Ok(getEmployeeViewModel);
        }

        // GET: api/auth/GetRole
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("GetRole")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsUser")]
        public async Task<ActionResult<string>> GetRole()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound();
            }
            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId.Value);
            if (userRole == null)
            {
                return NotFound();
            }

            if (userRole.RoleId == "1")
            {
                return Ok("Customer");
            } else if (userRole.RoleId == "2")
            {
                return Ok("Employee");
            } else if (userRole.RoleId == "3")
            {
                return Ok("Admin");
            }

            return BadRequest();
        }

        // GET: api/auth/Employee/5/10
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("Employee/{from}/{to}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsAdmin")]
        public async Task<ActionResult<ICollection<GetEmployeeViewModel>>> GetEmployee(int from, int to)
        {
            if (to <= from)
            {
                return BadRequest("to must be greater then from");
            }
            StringBuilder query = new StringBuilder();
            query.AppendLine("  select * from AspNetUsers where Id IN (");
            query.AppendLine("  select R.UserId from AspNetUserRoles R");
            query.AppendLine("  where R.RoleId = 2");
            query.AppendLine("  )");

            var employees = await _context.Users.FromSqlRaw(query.ToString())
                .OrderBy(c => c.Id)
                .Skip(from)
                .Take(to - from)
                .ToListAsync();

            var getEmployeeViewModel = new List<GetEmployeeViewModel>();
            foreach (var employee in employees)
            {
                if (employee == null)
                {
                    return NotFound("Employee doesn't exist");
                }
                getEmployeeViewModel.Add(new GetEmployeeViewModel(employee));
            }
            return Ok(getEmployeeViewModel);
        }

        // PUT: api/auth/Customer
        [Microsoft.AspNetCore.Mvc.HttpPut]
        [Microsoft.AspNetCore.Mvc.Route("Customer")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsEmployee")]
        public async Task<ActionResult<GetCustomerViewModel>> PutCustomer([Microsoft.AspNetCore.Mvc.FromBody] PutCustomerViewModel putCustomerViewModel, Guid? customerId = null)
        {
            string id;
            if (customerId == null)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier);
                id = userId.Value;
            }
            else
            {
                id = customerId.ToString();
            }

            var customer = await _context.Users
                .Include(c => c.DeliveryAddress)
                .Include(c => c.InvoiceAddress)
                .Include(c => c.Orders)
                .Include(c => c.InvoiceAddress.City)
                .Include(c => c.DeliveryAddress.City)
                .FirstOrDefaultAsync(c => c.Id == id);



            customer.FirstName = putCustomerViewModel.FirstName ?? customer.FirstName;
            customer.LastName = putCustomerViewModel.LastName ?? customer.LastName;
            customer.PhoneNumber = putCustomerViewModel.PhoneNumber ?? customer.PhoneNumber;
            customer.Email = putCustomerViewModel.Email ?? customer.Email;
            customer.IsDisabled = putCustomerViewModel.IsDisabled ?? customer.IsDisabled;

            if (putCustomerViewModel.DeliveryAddress != null)
            {
                customer.DeliveryAddress.State =
                    putCustomerViewModel.DeliveryAddress.State ?? customer.DeliveryAddress.State;
                customer.DeliveryAddress.Country =
                    putCustomerViewModel.DeliveryAddress.Country ?? customer.DeliveryAddress.Country;
                customer.DeliveryAddress.Street =
                    putCustomerViewModel.DeliveryAddress.Street ?? customer.DeliveryAddress.Street;
                customer.DeliveryAddress.StreetNumber =
                    putCustomerViewModel.DeliveryAddress.StreetNumber ?? customer.DeliveryAddress.StreetNumber;
                customer.DeliveryAddress.City.Name =
                    putCustomerViewModel.DeliveryAddress.CityName ?? customer.DeliveryAddress.City.Name;
                customer.DeliveryAddress.City.PostalCode=
                    putCustomerViewModel.DeliveryAddress.PostalCode ?? customer.DeliveryAddress.City.PostalCode;
            }

            if (putCustomerViewModel.InvoiceAddress != null)
            {
                customer.InvoiceAddress.State =
                    putCustomerViewModel.InvoiceAddress.State ?? customer.InvoiceAddress.State;
                customer.InvoiceAddress.Country =
                    putCustomerViewModel.InvoiceAddress.Country ?? customer.InvoiceAddress.Country;
                customer.InvoiceAddress.Street =
                    putCustomerViewModel.InvoiceAddress.Street ?? customer.InvoiceAddress.Street;
                customer.InvoiceAddress.StreetNumber =
                    putCustomerViewModel.InvoiceAddress.StreetNumber ?? customer.InvoiceAddress.StreetNumber;
                customer.InvoiceAddress.City.Name =
                    putCustomerViewModel.InvoiceAddress.CityName ?? customer.InvoiceAddress.City.Name;
                customer.InvoiceAddress.City.PostalCode =
                    putCustomerViewModel.InvoiceAddress.PostalCode ?? customer.InvoiceAddress.City.PostalCode;
            }

            _context.Entry(customer).State = EntityState.Modified;
            _context.Entry(customer.DeliveryAddress).State = EntityState.Modified;
            _context.Entry(customer.InvoiceAddress).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                //TODO: proper error handling
                Console.WriteLine(e);
                throw;
            }

            var getCustomerViewModel = new GetCustomerViewModel(customer);
            return Ok(getCustomerViewModel);
        }

        //POST: api/auth/Customer/ResetPassword
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("Customer/ResetPassword")]
        [Microsoft.AspNetCore.Authorization.Authorize(Policy = "IsActiveCustomer")]
        public async Task<IActionResult> ResetPasswordCustomer([Microsoft.AspNetCore.Mvc.FromBody] ResetPasswordViewModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound("User not Found");
            }
            model.CustomerId = userId.Value;
            var result = await _customerService.ResetPasswordAsync(model);
            if (result.IsSuccess)
            {
                return Ok();
            }
            return BadRequest(result.Message);
        }


        //POST: api/auth/Customer/ForgotPassword
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("Customer/ForgotPassword")]
        public async Task<IActionResult> ForgotPasswordCustomer(string customerEmail)
        {
            var result = await _customerService.ForgetPasswordAsync(customerEmail);
            if (result.IsSuccess)
            {
                return Ok();
            }
            return BadRequest(result.Message);
        }

        private bool validateCustomerWithEmail(string customerEmail)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == customerEmail);
            if (user == null)
            {
                return false;
            }
            return !user.IsDisabled;
        }

        private bool validateCustomerWithId(string customerID)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == customerID);
            if (user == null)
            {
                return false;
            }
            return !user.IsDisabled;
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
