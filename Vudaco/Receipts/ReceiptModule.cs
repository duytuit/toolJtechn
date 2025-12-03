using Microsoft.Extensions.DependencyInjection;
using Vudaco.Receipts.Repositories;

namespace Vudaco.Receipts
{
    public static class ReceiptModule
    {
        public static IServiceCollection AddReceiptModule(this IServiceCollection services)
        {
            services.AddScoped<IReceiptDetailRepositories, ReceiptDetailRepositories>();
            services.AddScoped<IReceiptRepositories, ReceiptRepositories>();
            services.AddScoped<IOffsetRepositories, OffsetRepositories>();
            return services;
        }
    }
}
