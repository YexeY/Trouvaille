using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Models.Communication;
using AuthoDemoMVC.Models.ViewModels;

namespace AuthoDemoMVC.Data.CustomerService
{
    public interface ICustomerService
    {
        Task<UserManagerResponse> RegisterCustomerAsync(RegisterCustomerViewModel model);

        Task<UserManagerResponse> LoginCustomerAsync(LoginCustomerViewModel model);
    }
}
