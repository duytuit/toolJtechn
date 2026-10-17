using System;
using System.Threading.Tasks;
using System.Security.Claims;
using JtechnApi.Shares.BaseRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JtechnApi.Users.Authorization
{
    public static class PermissionClaims
    {
        public const string Permission = "permission";
        public const string PolicyPrefix = "Permission:";
    }

    public sealed class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permissionCode)
        {
            PermissionCode = permissionCode;
        }

        public string PermissionCode { get; }
    }

    public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return _fallbackProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
        {
            return _fallbackProvider.GetFallbackPolicyAsync();
        }

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName != null &&
                policyName.StartsWith(PermissionClaims.PolicyPrefix, StringComparison.Ordinal))
            {
                var permissionCode = policyName.Substring(PermissionClaims.PolicyPrefix.Length);
                if (!string.IsNullOrWhiteSpace(permissionCode))
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .AddRequirements(new PermissionRequirement(permissionCode))
                        .Build();
                    return Task.FromResult(policy);
                }
            }

            return _fallbackProvider.GetPolicyAsync(policyName);
        }
    }

    public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly JtechnApi.Users.Repositories.IPermissionRepository repository;

        public PermissionAuthorizationHandler(JtechnApi.Users.Repositories.IPermissionRepository repository)
        {
            this.repository = repository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (!int.TryParse(
                    context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                    out var employeeId))
            {
                return;
            }

            if (await repository.HasPermission(employeeId, requirement.PermissionCode))
            {
                context.Succeed(requirement);
            }
        }
    }
}
