
using JtechnApi.Users.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JtechnApi.Users
{
    public static class UsersModule
    {
        public static IServiceCollection AddUsersModule(this IServiceCollection services)
        {
            services.AddScoped<IAdminRepository, AdminRepository>();
            return services;
        }
    }
}