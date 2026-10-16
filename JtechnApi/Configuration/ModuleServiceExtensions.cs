using JtechnApi.Accessorys;
using JtechnApi.Departments;
using JtechnApi.Employees;
using JtechnApi.Exams;
using JtechnApi.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JtechnApi.Configuration
{
    public static class ModuleServiceExtensions
    {
        public static IServiceCollection AddBusinessModules(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAccessorysModule();
            services.AddDepartmentsModule();
            services.AddExamsModule();
            services.AddUsersModule();
            services.AddEmployeesModule();

            return services;
        }

    }
}
