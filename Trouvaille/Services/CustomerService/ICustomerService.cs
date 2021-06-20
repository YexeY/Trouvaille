using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Models;
using AuthoDemoMVC.Models.Communication;
using AuthoDemoMVC.Models.ViewModels;
using Trouvaille.Models.Communication.Base;
using Trouvaille.Models.Communication.Customer;

namespace AuthoDemoMVC.Data.CustomerService
{
    public interface ICustomerService
    {
        Task<UserManagerResponse> RegisterCustomerAsync(RegisterCustomerViewModel model);

        Task<UserManagerResponse> LoginCustomerAsync(LoginCustomerViewModel model);

        Task<GetCustomerViewModel> GetCustomerInfo(string customerId);

        Task<UserManagerResponse> ForgetPasswordAsync(string email);

        Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordViewModel model);
    }
}
