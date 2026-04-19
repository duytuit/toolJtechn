using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Bills.Repositories;
using Vudaco.Depreciations.Repositories;

namespace Vudaco.Depreciations
{
    public static class DepreciationModule
    {
        public static IServiceCollection AddDepreciationModule(this IServiceCollection services)
        {
            services.AddScoped<IDepreciationRepositories, DepreciationRepositories>();
            services.AddScoped<IDepreciationAllocationRepositories, DepreciationAllocationRepositories>();
            services.AddScoped<IDepreciationAllocationDetailRepositories, DepreciationAllocationDetailRepositories>();
            return services;
        }
    }
}
