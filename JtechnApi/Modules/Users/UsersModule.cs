
using JtechnApi.Users.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using JtechnApi.Users.Repositories;

namespace JtechnApi.Users
{
    public static class UsersModule
    {
        public static IServiceCollection AddUsersModule(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            return services;
        }
    }
}