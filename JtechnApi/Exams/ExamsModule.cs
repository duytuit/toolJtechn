
using JtechnApi.Exams.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JtechnApi.Exams
{
    public static class ExamsModule
    {
        public static IServiceCollection AddExamsModule(this IServiceCollection services)
        {
            services.AddScoped<IExamRepository, ExamRepository>();
            return services;
        }
    }
}