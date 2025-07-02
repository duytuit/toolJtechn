using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pdftopng.Services;

namespace pdftopng
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
       
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
              .ConfigureServices((hostContext, services) =>
              {
                  // Register WebSocketClientService
                  services.AddSingleton(new Uri("wss://192.168.207.6:5007/ws"));
                  services.AddSingleton<WebSocketClientService>();
                  services.AddHostedService(provider => provider.GetRequiredService<WebSocketClientService>());
                  //services.AddSingleton<IHostedService>(provider =>new WebSocketClientService(new Uri("wss://192.168.207.6:5007/ws")));
              }).ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        // Cấu hình KeepAliveTimeout
                        options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2); // Ví dụ: Đặt KeepAliveTimeout là 2 phút
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
