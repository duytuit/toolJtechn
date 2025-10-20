using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Activitys.Repositories;

namespace Vudaco.Activitys
{
    public static class ActivityModule
    {
        public static IServiceCollection AddActivityModule(this IServiceCollection services)
        {
            services.AddScoped<IActivityRepositories, ActivityRepositories>();
            return services;
        }
    }
}
