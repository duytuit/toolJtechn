
using JtechnApi.Employees.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JtechnApi.Employees
{
    public static class EmployeesModule
    {
        public static IServiceCollection AddEmployeesModule(this IServiceCollection services)
        {
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeDepartmentRepository, EmployeeDepartmentRepository>();
            return services;
        }
    }
}
