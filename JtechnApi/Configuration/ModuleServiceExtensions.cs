using JtechnApi.Accessorys;
using JtechnApi.Departments;
using JtechnApi.Employees;
using JtechnApi.Exams;
using JtechnApi.Modules.AssemblyTargetStatistics.Repositories;
using JtechnApi.Modules.CutProduction.Repositories;
using JtechnApi.Modules.PlugProduction.Repositories;
using JtechnApi.Modules.StampingWireStatistics.Repositories;
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
            services.AddScoped<AssemblyCombineTargetStatisticsRepository>();
            services.AddScoped<StampingWireProductionRepository>();
            services.AddScoped<CutProductionRepository>();
            services.AddScoped<PlugProductionRepository>();

            return services;
        }

    }
}
