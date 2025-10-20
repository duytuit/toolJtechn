using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Debits.Repositories;

namespace Vudaco.Debits
{
    public static class DebitModule
    {
        public static IServiceCollection AddDebitModule(this IServiceCollection services)
        {
            services.AddScoped<IDebitRepositories, DebitRepositories>();
            return services;
        }
    }
}
