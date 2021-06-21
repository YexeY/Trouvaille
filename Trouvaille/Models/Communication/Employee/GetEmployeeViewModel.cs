using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AuthoDemoMVC.Models;

namespace Trouvaille.Models.Communication.Employee
{
    public class GetEmployeeViewModel
    {
        [StringLength(50)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        public GetEmployeeViewModel(ApplicationUser model)
        {
            Email = model.Email;
            FirstName = model.FirstName;
            LastName = model.LastName;
        }
    }
}
