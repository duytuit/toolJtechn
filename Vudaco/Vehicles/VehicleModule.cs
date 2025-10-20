using Microsoft.Extensions.DependencyInjection;
using Vudaco.Vehicles.Repositories;

namespace Vudaco.Vehicles
{
    public static class VehicleModule
    {
        public static IServiceCollection AddVehicleModule(this IServiceCollection services)
        {
            services.AddScoped<IVehicleDispatchRepositories, VehicleDispatchRepositories>();
            services.AddScoped<IVehicleRepositories, VehicleRepositories>();
            return services;
        }
    }
}
