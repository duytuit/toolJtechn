
using JtechnApi.BorrowProducts.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JtechnApi.BorrowProducts
{
    public static class BorrowProductModule
    {
        public static IServiceCollection AddBorrowProductModule(this IServiceCollection services)
        {
            services.AddScoped<IBorrowProductRepository, BorrowProductRepository>();
            return services;
        }
    }
}