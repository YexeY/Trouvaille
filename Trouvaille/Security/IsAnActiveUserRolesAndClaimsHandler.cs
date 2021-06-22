using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthoDemoMVC.Data;
using Microsoft.AspNetCore.Authorization;

namespace Trouvaille.Security
{
    public class IsAnActiveUserRolesAndClaimsHandler : AuthorizationHandler<ManageUserRolesAndClaimsRequirements>
    {
        private readonly ApplicationDbContext _dbContext;
        public IsAnActiveUserRolesAndClaimsHandler(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageUserRolesAndClaimsRequirements requirement)
        {
            /**
            if (!(context.Resource is AuthorizationFilterContext authFilterContext))
            {
                return Task.CompletedTask;
            }
            **/

            var loggedInCustomerId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var roles = _dbContext.UserRoles.FirstOrDefault(ur => ur.UserId.Equals(loggedInCustomerId));
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == loggedInCustomerId);
            if (roles == null || user == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            if (roles.RoleId == "1" && user.IsDisabled == false)
            {
                context.Succeed(requirement);
            } else if (roles.RoleId == "2" || roles.RoleId == "3")
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
