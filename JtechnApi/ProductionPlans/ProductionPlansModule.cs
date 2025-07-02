
using JtechnApi.ProductionPlans.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JtechnApi.ProductionPlans
{
    public static class ProductionPlansModule
    {
        public static IServiceCollection AddProductionPlansModule(this IServiceCollection services)
        {
            services.AddScoped<IProductionPlanRepository, ProductionPlanRepository>();
            return services;
        }
    }
}