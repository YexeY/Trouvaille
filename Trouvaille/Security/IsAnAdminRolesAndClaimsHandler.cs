using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthoDemoMVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Trouvaille.Security
{
    public class IsAnAdminRolesAndClaimsHandler : AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
    {
        private readonly ApplicationDbContext _dbContext;
        public IsAnAdminRolesAndClaimsHandler(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageAdminRolesAndClaimsRequirement requirement)
        {
            var authFilterContext = context.Resource as AuthorizationFilterContext;
            if (authFilterContext == null)
            {
                return Task.CompletedTask;
            }

            var loggedInAdminId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var roles = _dbContext.UserRoles.FirstOrDefault(ur => ur.UserId.Equals(loggedInAdminId));
            if (roles.RoleId == "3")
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
