using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthoDemoMVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Trouvaille.Security
{
    public class IsAnActiveCustomerRolesAndClaimsHandler : AuthorizationHandler<ManageCustomerRolesAndClaimsRequirement>
    {
        private readonly ApplicationDbContext _dbContext;
        public IsAnActiveCustomerRolesAndClaimsHandler(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageCustomerRolesAndClaimsRequirement requirement)
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
            if (roles.RoleId == "1" && user.IsDisabled == false)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
