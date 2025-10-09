using System;
using System.Threading;
using System.Threading.Tasks;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class WarmUpService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public WarmUpService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DBContext>();

        // Truy vấn "nhẹ" để EF mở kết nối, load model
        await db.Admin.FirstOrDefaultAsync();

        // Gọi các API ngoài hoặc preload cache nếu cần
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}