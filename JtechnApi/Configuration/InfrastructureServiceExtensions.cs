using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.Shares.Connects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using StackExchange.Redis;

namespace JtechnApi.Configuration
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DeepLSettings>(configuration.GetSection("DeepL"));
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConfiguration = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis"), true);
                redisConfiguration.ResolveDns = true;
                return ConnectionMultiplexer.Connect(redisConfiguration);
            });
            ConnectionStrings con = new ConnectionStrings();
            configuration.Bind("ConnectionStrings", con);
            services.AddSingleton(con);
            services.Configure<ConnectionStrings>(configuration.GetSection("ConnectionStrings"));
            services.AddScoped<AdoJtecDB_Khoa>();
            services.AddScoped<AdoJtecDB_KhoaB>();
            if (string.IsNullOrWhiteSpace(con.JtecDB_Default))
            {
                throw new System.InvalidOperationException(
                    "Thiếu cấu hình ConnectionStrings:JtecDB_Default cho DBJtecContext.");
            }
            services.AddDbContext<DBJtecContext>(options =>
                options.UseSqlServer(con.JtecDB_Default));
            services.AddDbContext<DBContext>(o => o.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection")),
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

            services.AddScoped<OracleConnection>(sp =>
            {
                var factory = sp.GetRequiredService<OracleConnectionFactory>();
                return factory.CreateConnection();
            });
            services.AddSingleton<OracleConnectionFactory>(); // chỉ chứa connection string

            return services;
        }

    }
}
