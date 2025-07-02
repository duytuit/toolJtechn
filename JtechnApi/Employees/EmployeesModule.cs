
using JtechnApi.Employees.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JtechnApi.Employees
{
    public static class EmployeesModule
    {
        public static IServiceCollection AddEmployeesModule(this IServiceCollection services)
        {
            services.AddScoped<IEmployeeDepartmentRepository, EmployeeDepartmentRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            return services;
        }
    }
}