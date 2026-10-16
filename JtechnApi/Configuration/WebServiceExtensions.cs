using JtechnApi.Shares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JtechnApi.Configuration
{
    public static class WebServiceExtensions
    {
        public static IServiceCollection AddWebSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin() // hoặc .WithOrigins("https://your-frontend.com")
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/login";
                        options.LogoutPath = "/logout";
                        options.AccessDeniedPath = "/access-denied";
                        options.ExpireTimeSpan = System.TimeSpan.FromHours(8);
                        options.SlidingExpiration = true;
                        options.Cookie.HttpOnly = true;
                        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                        options.Events.OnRedirectToLogin = context =>
                        {
                            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return System.Threading.Tasks.Task.CompletedTask;
                            }

                            context.Response.Redirect(context.RedirectUri);
                            return System.Threading.Tasks.Task.CompletedTask;
                        };
                        options.Events.OnRedirectToAccessDenied = context =>
                        {
                            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                            {
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                return System.Threading.Tasks.Task.CompletedTask;
                            }

                            context.Response.Redirect(context.RedirectUri);
                            return System.Threading.Tasks.Task.CompletedTask;
                        };
                    });
            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection AddWebPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ValidateModelFilter>();
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            services.AddScoped<RedisService>();
            services.AddControllers(options =>
            {
                options.Filters.Add<ValidateModelFilter>();
            });
            services.AddHttpContextAccessor();
            services.AddControllersWithViews(); // 👈 hỗ trợ cả View + API

            return services;
        }

        public static IServiceCollection AddWebUtilities(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient(); // Add HttpClient factory
            services.AddResponseCompression();
            return services;
        }

    }
}
