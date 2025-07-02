
using JtechnApi.Umesens.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JtechnApi.Umesens
{
    public static class UmesensModule
    {
        public static IServiceCollection AddUmesensModule(this IServiceCollection services)
        {
            services.AddScoped<IUmesenRepository, UmesenRepository>();
            return services;
        }
    }
}