using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vudaco.Notifys.Repositories;

namespace Vudaco.Shares
{
    public class FcmBackgroundWorker : BackgroundService
    {
        private readonly IFcmQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;

        public FcmBackgroundWorker(IFcmQueue queue, IServiceScopeFactory scopeFactory)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var job = await _queue.DequeueAsync(stoppingToken);

                // mỗi job chạy 1 scope riêng => DbContext không bị disposed
                using var scope = _scopeFactory.CreateScope();

                var fcmService = scope.ServiceProvider.GetRequiredService<FcmService>();

                try
                {
                    await fcmService.SendMulticastAsync(
                        job.UserIds,
                        job.Title,
                        job.Body,
                        job.StorageId,
                        job.PostId,
                        job.Type,
                        job.Screen,
                        job.Data
                    );
                }
                catch (Exception ex)
                {
                    // TODO: log ra DB / file / telegram
                    Console.WriteLine("FCM Background Worker error: " + ex.Message);
                }
            }
        }
    }
}
