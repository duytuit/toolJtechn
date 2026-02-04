using Microsoft.Extensions.DependencyInjection;
using Vudaco.SendMails.Repositories;

namespace Vudaco.SendMails
{
    public static class SendMailModule
    {
        public static IServiceCollection AddSendMailModule(this IServiceCollection services)
        {
            services.AddScoped<IEmailTemplateRepositories, EmailTemplateRepositories>();
            services.AddScoped<ISmtpSettingRepositories, SmtpSettingRepositories>();
            return services;
        }
    }
}
