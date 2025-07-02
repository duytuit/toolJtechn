
using JtechnApi.Departments.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JtechnApi.Departments
{
    public static class DepartmentsModule
    {
        public static IServiceCollection AddDepartmentsModule(this IServiceCollection services)
        {
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            return services;
        }
    }
}