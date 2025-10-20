using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Bills.Repositories;

namespace Vudaco.Bills
{
    public static class BillModule
    {
        public static IServiceCollection AddBillModule(this IServiceCollection services)
        {
            services.AddScoped<IBillRepositories, BillRepositories>();
            return services;
        }
    }
}
