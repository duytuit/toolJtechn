using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Auth.Repositories;
using Vudaco.ContractFiles.Repositories;
using Vudaco.Shares;

namespace Vudaco.ContractFiles
{
    public static class ContractFileModule
    {
        public static IServiceCollection AddContractFileModule(this IServiceCollection services)
        {
            services.AddScoped<IContractFileRepository, ContractFileRepository>();
            services.AddScoped<IContractFileDetailRepository, ContractFileDetailRepository>();
            return services;
        }
    }
}
