using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Partners.Repositories;

namespace Vudaco.Partners
{
    public static class PartnerModule
    {
        public static IServiceCollection AddPartnerModule(this IServiceCollection services)
        {
            services.AddScoped<IPartnerDetailRepository, PartnerDetailRepository>();
            services.AddScoped<IPartnerRepository, PartnerRepository>();
            return services;
        }
    }
}
