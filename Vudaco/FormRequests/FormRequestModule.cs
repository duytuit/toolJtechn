using Microsoft.Extensions.DependencyInjection;
using Vudaco.FormRequests.Repositories;

namespace Vudaco.FormRequests
{
    public static class FormRequestModule
    {
        public static IServiceCollection AddFormRequestModule(this IServiceCollection services)
        {
            services.AddScoped<IFormRequestRepositories, FormRequestRepositories>();
            return services;
        }
    }
}
