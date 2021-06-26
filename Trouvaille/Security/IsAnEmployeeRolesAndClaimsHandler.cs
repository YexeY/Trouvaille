using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthoDemoMVC.Data;
using Microsoft.AspNetCore.Authorization;

namespace Trouvaille.Security
{
    public class IsAnEmployeeRolesAndClaimsHandler : AuthorizationHandler<ManageEmployeeRolesAndClaimsRequirements>
    {
        private readonly ApplicationDbContext _dbContext;
        public IsAnEmployeeRolesAndClaimsHandler(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageEmployeeRolesAndClaimsRequirements requirement)
        {
            var loggedInCustomerId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var roles = _dbContext.UserRoles.FirstOrDefault(ur => ur.UserId.Equals(loggedInCustomerId));
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == loggedInCustomerId);
            if (roles == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            if (roles.RoleId == "2"  || roles.RoleId == "3")
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
