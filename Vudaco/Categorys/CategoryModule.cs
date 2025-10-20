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
            services.AddScoped<IAdministrativeFeeRepository, AdministrativeFeeRepository>();
            services.AddScoped<IBankRepository, BankRepository>();
            services.AddScoped<IBranchCategoryRepository, BranchCategoryRepository>();
            services.AddScoped<IFundCategoryRepository, FundCategoryRepository>();
            services.AddScoped<IHouseholdFeeCategoryRepository, HouseholdFeeCategoryRepository>();
            services.AddScoped<IIncomeCategoryRepository, IncomeCategoryRepository>();
            services.AddScoped<IPriceCategoryRepository, PriceCategoryRepository>();
            return services;
        }
    }
}
