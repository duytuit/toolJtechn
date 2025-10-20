using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Receipts.Repositories;

namespace Vudaco.Receipts
{
    public static class ReceiptModule
    {
        public static IServiceCollection AddReceiptModule(this IServiceCollection services)
        {
            services.AddScoped<IReceiptDetailRepositories, ReceiptDetailRepositories>();
            services.AddScoped<IReceiptRepositories, ReceiptRepositories>();
            return services;
        }
    }
}
