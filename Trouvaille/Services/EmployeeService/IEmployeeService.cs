using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Models.Communication;
using AuthoDemoMVC.Models.ViewModels;

namespace AuthoDemoMVC.Data.EmployeeService
{
    public interface IEmployeeService
    {
        Task<UserManagerResponse> RegisterEmployeeAsync(RegisterEmployeeViewModel model);

        Task<UserManagerResponse> LoginEmployeeAsync(LoginEmployeeViewModel model);
    }
}
