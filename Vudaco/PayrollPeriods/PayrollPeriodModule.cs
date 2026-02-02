using Microsoft.Extensions.DependencyInjection;
using Vudaco.PayrollPeriods.Repositories;

namespace Vudaco.PayrollPeriods
{
    public static class PayrollPeriodModule
    {
        public static IServiceCollection AddPayrollPeriodModule(this IServiceCollection services)
        {
            services.AddScoped<IPayrollPeriodRepositories, PayrollPeriodRepositories>();
            services.AddScoped<IPayrollPeriodDetailRepositories, PayrollPeriodDetailRepositories>();
            return services;
        }
    }
}
