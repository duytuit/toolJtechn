
using JtechnApi.Requireds.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JtechnApi.Requireds
{
    public static class RequiredsModule
    {
        public static IServiceCollection AddRequiredsModule(this IServiceCollection services)
        {
            services.AddScoped<ISignatureSubmissionRepository, SignatureSubmissionRepository>();
            services.AddScoped<IRequiredRepository, RequiredRepository>();
            services.AddScoped<ITempRequiredRepository, TempRequiredRepository>();
            return services;
        }
    }
}