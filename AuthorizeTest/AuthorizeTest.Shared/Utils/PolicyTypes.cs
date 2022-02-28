using System.Security.Claims;
using AuthorizeTest.Shared.Enums;
using AuthorizeTest.Shared.Models;
using Microsoft.AspNetCore.Authorization; // dotnet add package Microsoft.AspNetCore.Authorization

namespace AuthorizeTest.Shared.Utils
{
    public static class PolicyTypes
    {
        public const string RequireAdmin = nameof(RequireAdmin);
        public const string RequireCustomer = nameof(RequireCustomer);
        public const string RequireEmployee = nameof(RequireEmployee);
        public const string RequireEmployeeOrCustomer = nameof(RequireEmployeeOrCustomer);
        public const string RequireUser = nameof(RequireUser);
        public const string RequireEditor = nameof(RequireEditor);
        public static AuthorizationOptions AddAppPolicies(this AuthorizationOptions options)
        {
            options.AddPolicy(RequireAdmin, policy => policy.RequireRole(RolesEnum.Admin.ToString()));
            options.AddPolicy(RequireCustomer, policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ClaimTypes.Role
                            && (claim.Value == RolesEnum.Admin.ToString() || claim.Value == RolesEnum.Customer.ToString()))
                    ));
            options.AddPolicy(RequireEmployee, policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ClaimTypes.Role
                            && (claim.Value == RolesEnum.Admin.ToString() || claim.Value == RolesEnum.Employee.ToString()))
                    ));

            options.AddPolicy(RequireEmployeeOrCustomer, policy =>
                                policy.RequireAssertion(context =>
                                    context.User.HasClaim(claim => claim.Type == ClaimTypes.Role
                                        && (claim.Value == RolesEnum.Admin.ToString() ||
                                            claim.Value == RolesEnum.Employee.ToString() ||
                                            claim.Value == RolesEnum.Customer.ToString()))
                                ));
            options.AddPolicy(RequireUser, policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ClaimTypes.Role
                            && (claim.Value == RolesEnum.Admin.ToString() || claim.Value == RolesEnum.User.ToString()))
                    ));
            options.AddPolicy(RequireEditor, policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim => claim.Type == ClaimTypes.Role
                            && (claim.Value == RolesEnum.Admin.ToString() || claim.Value == RolesEnum.Editor.ToString()))
                    ));
            return options;
        }
    }
}
