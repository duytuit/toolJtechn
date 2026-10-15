using JtechnApi.Accessorys;
using JtechnApi.Departments;
using JtechnApi.Employees;
using JtechnApi.Exams;
using JtechnApi.Infra;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.Shares.Connects;
using JtechnApi.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using StackExchange.Redis;
using System;

namespace JtechnApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
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
            // cấu hình DeepLSettings
            services.Configure<DeepLSettings>(Configuration.GetSection("DeepL"));
            // kết nối redis
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(Configuration.GetConnectionString("Redis"), true);
                configuration.ResolveDns = true;
                return ConnectionMultiplexer.Connect(configuration);
            });
            // kết nối oracle 
            ConnectionStrings con = new ConnectionStrings();
            Configuration.Bind("ConnectionStrings", con);
            services.AddSingleton(con);
            services.Configure<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));
            services.AddScoped<AdoJtecDB_Khoa>();
            if (string.IsNullOrWhiteSpace(con.JtecDB_Default))
            {
                throw new System.InvalidOperationException(
                    "Thiếu cấu hình ConnectionStrings:JtecDB_Default cho DBJtecContext.");
            }
            services.AddDbContext<DBJtecContext>(options =>
                options.UseSqlServer(con.JtecDB_Default));
            services.AddDbContext<DBContext>(o => o.UseMySql(
                Configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("DefaultConnection")),
                mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: System.TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            }).UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
                .EnableSensitiveDataLogging()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            );

            // Khởi tạo kết nối Oracle
            services.AddScoped<OracleConnection>(sp =>
            {
                var factory = sp.GetRequiredService<OracleConnectionFactory>();
                return factory.CreateConnection();
            });
            services.AddSingleton<OracleConnectionFactory>(); // chỉ chứa connection string
                                                              // services.AddHostedService<WarmUpService>();
            services.AddScoped<ValidateModelFilter>();
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
            services.AddScoped<RedisService>();
            // khởi tạo Repository
            services.AddControllers(options =>
            {
                options.Filters.Add<ValidateModelFilter>();
            });
            services.AddHttpContextAccessor();
            services.AddControllersWithViews(); // 👈 hỗ trợ cả View + API
            services.AddAccessorysModule();
            services.AddDepartmentsModule();
            services.AddExamsModule();
            services.AddUsersModule();
            services.AddEmployeesModule();
            services.AddHttpClient(); // Add HttpClient factory
            services.AddResponseCompression();
            //services.AddResponseCompression(options =>
            //{
            //    options.Providers.Add<GzipCompressionProvider>();
            //    options.EnableForHttps = true;
            //});
            //services.AddSharesModule();
            //services.AddResponseCompression(); // Bật nén dữ liệu trả về (nếu cần)
            //services.AddGzipCompressionProvider(); // Cung cấp nén Gzip (nếu cần)
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
          
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowAll"); // Quan trọng: phải đặt trước UseAuthorization nếu có
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseStaticFiles(); // Cho phép truy cập các file tĩnh trong wwwroot
            app.UseResponseCompression();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllers(); // API route
            });
        }
    }
}
