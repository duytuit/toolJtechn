using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Departments.Repositories;

namespace Vudaco.Departments
{
    public static class DepartmentModule
    {
        public static IServiceCollection AddDepartmentModule(this IServiceCollection services)
        {
            services.AddScoped<IDepartmentRepositories, DepartmentRepositories>();
            return services;
        }
    }
}
