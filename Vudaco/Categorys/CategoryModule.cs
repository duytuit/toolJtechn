using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Categorys.Repositories;

namespace Vudaco.Categorys
{
    public static class CategoryModule
    {
        public static IServiceCollection AddCategoryModule(this IServiceCollection services)
        {
            services.AddScoped<IIncomeExpenseCategoryRepository, IncomeExpenseCategoryRepository>();
            services.AddScoped<IBankRepository, BankRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IFundCategoryRepository, FundCategoryRepository>();
            return services;
        }
    }
}
