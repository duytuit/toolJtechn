using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Vudaco.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using Vudaco.Shares;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Vudaco.Middlewares;
using Vudaco.Auth;
using Vudaco.ContractFiles;
using Vudaco.Employees;
using Vudaco.Storages;
using Vudaco.Partners;
using Vudaco.Categorys;
using Vudaco.Activitys;
using Vudaco.Bills;
using Vudaco.Debits;
using Vudaco.Departments;
using Vudaco.Receipts;
using Vudaco.Vehicles;
using Vudaco.Shares.Connects;
using Microsoft.Extensions.Options;
using Vudaco.Comments;
using Vudaco.Notifys;
using Vudaco.FormRequests;
using Vudaco.PayrollPeriods;
using Vudaco.SendMails;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Vudaco.Notifys.Repositories;
using Vudaco.Depreciations;

namespace Vudaco
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
            // Init Firebase
            // if (FirebaseApp.DefaultInstance == null)
            // {
            //     FirebaseApp.Create(new AppOptions
            //     {
            //         Credential = GoogleCredential.FromFile("appvudaco-a5a65d4905b2.json"),
            //         ProjectId = "appvudaco"
            //     });
            // }
            services.AddSingleton<IFcmQueue, FcmQueue>();
            services.AddHostedService<FcmBackgroundWorker>();
            services.AddScoped<FcmService>();
            services.Configure<TelegramSettings>(Configuration.GetSection("Telegram"));
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
            // kết nối redis
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = ConfigurationOptions.Parse(Configuration.GetConnectionString("Redis"), true);
                configuration.ResolveDns = true;
                return ConnectionMultiplexer.Connect(configuration);
            });
            // kết nối sql server
            services.AddDbContext<VudacoDBContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );
            // kết nối sql server kiểu ado
            services.Configure<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));
            services.AddTransient<AdoVudacoDB>();
            // kết nối sql server vudaco cũ
            services.AddDbContext<VudacoOldDBContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnectionVuDaCo"))
            );
            services.AddSingleton<RedisService>();
            services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super-secret-key"))
                };
            });
            services.AddResponseCompression();
            services.AddControllersWithViews(); // 👈 hỗ trợ cả View + API
            services.AddHttpClient(); // Add HttpClient factory
            services.AddAuthModule();
            services.AddContractFileModule();
            services.AddEmployeeModule();
            services.AddStoragesModule();
            services.AddPartnerModule();
            services.AddCategoryModule();
            services.AddActivityModule();
            services.AddBillModule();
            services.AddDebitModule();
            services.AddDepartmentModule();
            services.AddReceiptModule();
            services.AddVehicleModule();
            services.AddCommentModule();
            services.AddNotifyModule();
            services.AddFormRequestModule();
            services.AddPayrollPeriodModule();
            services.AddSendMailModule();
            services.AddDepreciationModule();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var telegramConfig = app.ApplicationServices.GetRequiredService<IOptions<TelegramSettings>>();
            Helper.ConfigureTelegram(telegramConfig.Value);
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowAll"); // Quan trọng: phải đặt trước UseAuthorization nếu có

            app.UseMiddleware<ExceptionMiddleware>();

            // Check JWT + Redis
            app.UseMiddleware<JwtRedisMiddleware>();

            app.UseAuthorization();

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
