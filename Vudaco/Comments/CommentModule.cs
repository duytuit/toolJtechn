using Microsoft.Extensions.DependencyInjection;
using Vudaco.Comments.Repositories;

namespace Vudaco.Comments
{
    public static class CommentModule
    {
        public static IServiceCollection AddCommentModule(this IServiceCollection services)
        {
            services.AddScoped<ICommentRepositories, CommentRepositories>();
            return services;
        }
    }
}
