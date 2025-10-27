using Microsoft.Extensions.DependencyInjection;
using Vudaco.Employees.Repositories;

namespace Vudaco.Employees
{
    public static class EmployeeModule
    {
        public static IServiceCollection AddEmployeeModule(this IServiceCollection services)
        {
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeDepartmentRepository, EmployeeDepartmentRepository>();
            return services;
        }
    }
}
