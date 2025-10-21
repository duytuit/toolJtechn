using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Storages.Repositories;

namespace Vudaco.Storages
{
    public static class StoragesModule
    {
        public static IServiceCollection AddStoragesModule(this IServiceCollection services)
        {
            services.AddScoped<IStorageRepository, StorageRepository>();
            services.AddScoped<IUserStorageRepository, UserStorageRepository>();
            return services;
        }
    }
}
