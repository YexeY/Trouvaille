using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthoDemoMVC.Data.CustomerService;
using AuthoDemoMVC.Data.EmployeeService;
using AuthoDemoMVC.Data.UserService;
using AuthoDemoMVC.Models.Communication;
using AuthoDemoMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Trouvaille3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;

        public AuthController(IUserService userService, ICustomerService customerService, IEmployeeService employeeService)
        {
            _userService = userService;
            _customerService = customerService;
            _employeeService = employeeService;
        }

        // POST: api/auth/customer/register
        [HttpPost]
        [Route("Customer/Register")]
        public async Task<IActionResult> RegisterCustomerAsync([FromBody] RegisterCustomerViewModel model)
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
        [HttpPost]
        [Route("Customer/Login")]
        public async Task<IActionResult> LoginCustomerAsync([FromBody] LoginCustomerViewModel model)
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
        [HttpPost]
        [Route("Employee/Register")]
        public async Task<IActionResult> RegisterEmployeeAsync([FromBody] RegisterEmployeeViewModel model)
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
        [HttpPost]
        [Route("Employee/Login")]
        public async Task<IActionResult> LoginEmployeeAsync([FromBody] LoginEmployeeViewModel model)
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
        [HttpGet]
        [Route("Customer/info")]
        [Authorize]
        public async Task<IActionResult> GetCustomerInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            var result = await _customerService.GetCustomerInfo(userId.Value);

            return Ok(result);
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
