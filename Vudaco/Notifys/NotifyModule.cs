using Microsoft.Extensions.DependencyInjection;
using Vudaco.Notifys.Repositories;

namespace Vudaco.Notifys
{
    public static class NotifyModule
    {
        public static IServiceCollection AddNotifyModule(this IServiceCollection services)
        {
            services.AddScoped<INotifyRepositories, NotifyRepositories>();
            return services;
        }
    }
}
