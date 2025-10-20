using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Employees.Repositories;

namespace Vudaco.Employees
{
    public static class EmployeeModule
    {
        public static IServiceCollection AddEmployeeModule(this IServiceCollection services)
        {
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            return services;
        }
    }
}
