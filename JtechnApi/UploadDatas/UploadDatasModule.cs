
using JtechnApi.UploadDatas.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JtechnApi.UploadDatas
{
    public static class UploadDatasModule
    {
        public static IServiceCollection AddUploadDatasModule(this IServiceCollection services)
        {
            services.AddScoped<IUploadDataRepository, UploadDataRepository>();
            return services;
        }
    }
}