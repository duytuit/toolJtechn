
using JtechnApi.Accessorys.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JtechnApi.Accessorys
{
    public static class AccessorysModule
    {
        public static IServiceCollection AddAccessorysModule(this IServiceCollection services)
        {
            services.AddScoped<IAccessoryRepository, AccessoryRepository>();
            return services;
        }
    }
}