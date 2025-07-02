using JtechnApi.Accessorys;
using JtechnApi.Departments;
using JtechnApi.Employees;
using JtechnApi.Exams;
using JtechnApi.Infra;
using JtechnApi.ProductionPlans;
using JtechnApi.Requireds;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.Shares.Connects;
using JtechnApi.Umesens;
using JtechnApi.UploadDatas;
using JtechnApi.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using StackExchange.Redis;

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
            services.AddDbContext<DBContext>(o => o.UseMySql(Configuration.GetConnectionString("DefaultConnection"), mySqlOptions =>
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
            services.AddControllersWithViews(); // 👈 hỗ trợ cả View + API
            services.AddAccessorysModule();
            services.AddDepartmentsModule();
            services.AddUmesensModule();
            services.AddProductionPlansModule();
            services.AddExamsModule();
            services.AddRequiredsModule();
            services.AddUsersModule();
            services.AddUploadDatasModule();
            services.AddEmployeesModule();
            services.AddHttpClient(); // Add HttpClient factory
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

            app.UseMiddleware<ExceptionMiddleware>();
            
            //app.UseAuthorization();

            //app.UseResponseCompression();
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
