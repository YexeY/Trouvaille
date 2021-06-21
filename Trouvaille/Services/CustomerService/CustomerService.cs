using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthoDemoMVC.Models;
using AuthoDemoMVC.Models.Communication;
using AuthoDemoMVC.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Trouvaille.Models.Communication.Base;
using Trouvaille.Models.Communication.Customer;
using Trouvaille.Services.MailService;
using Trouvaille_WebAPI.Models;

namespace AuthoDemoMVC.Data.CustomerService
{
    public class CustomerService : ICustomerService
    {

        private readonly UserManager<ApplicationUser> _userManger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IMailService _mailService;


        public CustomerService(UserManager<ApplicationUser> userManager, IConfiguration configuration, ApplicationDbContext context, IMailService mailService)
        {
            _userManger = userManager;
            _configuration = configuration;
            _context = context;
            _mailService = mailService;
        }

        public async Task<UserManagerResponse> RegisterCustomerAsync(RegisterCustomerViewModel model)
        {
            if (model == null)
                throw new NullReferenceException("Register Model is null");

            if (model.Password != model.ConfirmPassword)
                return new UserManagerResponse
                {
                    Message = "Confirm password doesn't match the password",
                    IsSuccess = false,
                };

            //Create City
            var city = new City
            {
                CityId = Guid.NewGuid(),
                PostalCode = model.PostalCode,
                Name = model.CityName,
            };

            //Create Address
            var address = new Address
            {
                AddressId = Guid.NewGuid(),
                Country = model.Country,
                State = model.State,
                Street = model.Street,
                StreetNumber = model.StreetNumber,
                City = city
            };

            //Create Customer
            var customer = new ApplicationUser()
            {
                Email = model.Email,
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                InvoiceAddress = address,
                DeliveryAddress = address
            };

            var result = await _userManger.CreateAsync(customer, model.Password);

            if (!result.Succeeded)
            {
                return new UserManagerResponse
                {
                    Message = "User did not create!",
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description)
                };
            }

            //ADD USER THE ROLE CUSTOMER
            var userRole = new IdentityUserRole<string> {RoleId = "1", UserId = customer.Id};
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
            //-----------------------------


            if (result.Succeeded)
            {
                await _mailService.SendRegistrationConfirmationCustomerAsync(customer);
                //TODO error handling
                return new UserManagerResponse
                {
                    Message = "User created successfully!",
                    IsSuccess = true,
                };
            }

            return new UserManagerResponse
            {
                Message = "User did not create",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description)
            };

        }

        public async Task<UserManagerResponse> LoginCustomerAsync(LoginCustomerViewModel model)
        {
            var user = await _userManger.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return new UserManagerResponse
                {
                    Message = "There is no user with that Email address",
                    IsSuccess = false,
                };
            }

            var result = await _userManger.CheckPasswordAsync(user, model.Password);

            if (!result)
                return new UserManagerResponse
                {
                    Message = "Invalid password",
                    IsSuccess = false,
                };

            var userRole = _context.UserRoles.SingleOrDefault(ur => ur.UserId == user.Id);
            var role = _context.Roles.FirstOrDefault(x => x.Id == userRole.RoleId);

            var claims = new[]
            {
                new Claim("Email", model.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("Role", role.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthSettings:Issuer"],
                audience: _configuration["AuthSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);


            return new UserManagerResponse
            {
                Message = tokenAsString,
                IsSuccess = true,
                ExpireDate = token.ValidTo
            };
        }

        public async Task<GetCustomerViewModel> GetCustomerInfo(string customerId)
        {
            //var customer = await _context.Users.FindAsync(customerId);
            var customer = await _context.Users.Include(c => c.DeliveryAddress)
                .Include(c => c.InvoiceAddress)
                .Include(c => c.Orders)
                .Include(c => c.InvoiceAddress.City)
                .Include(c => c.DeliveryAddress.City)
                .FirstOrDefaultAsync(c => c.Id == customerId);
            var customerInfo = new GetCustomerViewModel(customer);

            return customerInfo;
        }

        /**
        public async Task<UserManagerResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManger.FindByIdAsync(userId);
            if (user == null)
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "User not found"
                };

            var decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManger.ConfirmEmailAsync(user, normalToken);

            if (result.Succeeded)
                return new UserManagerResponse
                {
                    Message = "Email confirmed successfully!",
                    IsSuccess = true,
                };

            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "Email did not confirm",
                Errors = result.Errors.Select(e => e.Description)
            };
        }
        **/
        
        public async Task<UserManagerResponse> ForgetPasswordAsync(string email)
        {
            var user = await _userManger.FindByEmailAsync(email);
            if (user == null)
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "No user associated with email",
                };

            /**
            var token = await _userManger.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Encoding.UTF8.GetBytes(token);
            var validToken = WebEncoders.Base64UrlEncode(encodedToken);
            **/

            var newPassword = GenerateRandomPassword();

            var resetPasswordViewModel = new ResetPasswordViewModel()
            {
                CustomerId = user.Id,
                NewPassword = newPassword,
                ConfirmPassword = newPassword
            };

            var userManagerResponse = await ResetPasswordAsync(resetPasswordViewModel);
            if (userManagerResponse.IsSuccess)
            {
                await _mailService.SendForgotPasswordEmailAsync(user, resetPasswordViewModel.NewPassword);
            }

            return await ResetPasswordAsync(resetPasswordViewModel);
        }
        
        public async Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await _userManger.FindByIdAsync(model.CustomerId);
            if (user == null)
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "No user associated with email",
                };

            if (model.NewPassword != model.ConfirmPassword)
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Password doesn't match its confirmation",
                };

            //----------------------------
            var token = await _userManger.GeneratePasswordResetTokenAsync(user);
            var result = await _userManger.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
                return new UserManagerResponse
                {
                    Message = "Password has been reset successfully!",
                    IsSuccess = true,
                };

            return new UserManagerResponse
            {
                Message = "Something went wrong",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description),
            };
        }

        private static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                                      || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

    }
}
