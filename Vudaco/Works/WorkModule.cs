using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Works.Repositories;

namespace Vudaco.Works
{
    public static class WorkModule
    {
        public static IServiceCollection AddWorkModule(this IServiceCollection services)
        {
            services.AddScoped<IWorkRepositories, WorkRepositories>();
            return services;
        }
    }
}
